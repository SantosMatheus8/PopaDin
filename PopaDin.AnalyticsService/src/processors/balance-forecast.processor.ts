import { Injectable, Logger } from '@nestjs/common';
import { RecordsRepository } from '../records/records.repository';
import {
  Insight,
  InsightProcessor,
  InsightSeverity,
  toDecimalValue,
  getMonthRange,
  getEndOfNextMonth,
  getFrequencyMonths,
} from '../analytics/interfaces/insight.interface';

@Injectable()
export class BalanceForecastProcessor implements InsightProcessor {
  private readonly logger = new Logger(BalanceForecastProcessor.name);

  constructor(private readonly recordsRepository: RecordsRepository) {}

  async process(userId: number): Promise<Insight[]> {
    try {
      const currentMonth = getMonthRange(0);

      const threeMonthsAgo = getMonthRange(2);
      const periodStart = threeMonthsAgo.start;
      const periodEnd = currentMonth.end;

      const [allRecords, recurringRecords] = await Promise.all([
        this.recordsRepository.getAllRecordsInPeriod(userId, periodStart, periodEnd),
        this.recordsRepository.getRecurringRecords(userId),
      ]);

      let currentBalance = 0;
      for (const record of allRecords) {
        const value = toDecimalValue(record.Value);
        currentBalance += record.Operation === 1 ? value : -value;
      }

      let monthlyRecurringIncome = 0;
      let monthlyRecurringExpenses = 0;

      for (const record of recurringRecords) {
        const value = toDecimalValue(record.Value);
        const freqMonths = getFrequencyMonths(record.Frequency);
        if (freqMonths === 0) continue;

        const monthlyValue = value / freqMonths;

        if (record.Operation === 1) {
          monthlyRecurringIncome += monthlyValue;
        } else {
          monthlyRecurringExpenses += monthlyValue;
        }
      }

      const oneTimeOutflows = allRecords.filter(
        (r) => r.Operation === 0 && r.Frequency === 5 && !r.InstallmentGroupId,
      );
      const totalOneTimeExpenses = oneTimeOutflows.reduce(
        (sum, r) => sum + toDecimalValue(r.Value),
        0,
      );
      const averageOneTimeExpenses = Math.round((totalOneTimeExpenses / 3) * 100) / 100;

      const monthlyNet = monthlyRecurringIncome - monthlyRecurringExpenses - averageOneTimeExpenses;
      const forecast = [1, 3, 6].map((months) => ({
        months,
        projected: Math.round((currentBalance + monthlyNet * months) * 100) / 100,
      }));

      let goesNegativeInMonths: number | null = null;
      if (monthlyNet < 0 && currentBalance > 0) {
        goesNegativeInMonths = Math.ceil(currentBalance / Math.abs(monthlyNet));
      }

      let severity: InsightSeverity = 'info';
      if (goesNegativeInMonths !== null && goesNegativeInMonths <= 3) {
        severity = 'critical';
      } else if (goesNegativeInMonths !== null && goesNegativeInMonths <= 6) {
        severity = 'warning';
      } else if (forecast.some((f) => f.projected < 0)) {
        severity = 'critical';
      }

      let message: string;
      if (goesNegativeInMonths !== null && goesNegativeInMonths <= 6) {
        message = `Atenção! Com base nos seus padrões, seu saldo pode ficar negativo em ${goesNegativeInMonths} meses`;
      } else {
        const threeMonthProjection = forecast.find((f) => f.months === 3);
        message = `Com base nos seus padrões, seu saldo em 3 meses será de aproximadamente R$ ${threeMonthProjection?.projected.toFixed(2).replace('.', ',')}`;
      }

      return [
        {
          userId,
          type: 'BALANCE_FORECAST',
          title: severity === 'critical' ? 'Alerta de Saldo Futuro' : 'Previsão de Saldo',
          message,
          severity,
          data: {
            currentBalance: Math.round(currentBalance * 100) / 100,
            forecast,
            goesNegativeInMonths,
            monthlyRecurringIncome: Math.round(monthlyRecurringIncome * 100) / 100,
            monthlyRecurringExpenses: Math.round(monthlyRecurringExpenses * 100) / 100,
            averageOneTimeExpenses,
          },
          period: {
            start: currentMonth.start,
            end: currentMonth.end,
          },
          expiresAt: getEndOfNextMonth(currentMonth.end),
        },
      ];
    } catch (error) {
      this.logger.error(`Erro ao processar previsão de saldo para userId=${userId}`, error);
      return [];
    }
  }
}
