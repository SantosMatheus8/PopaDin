import { Injectable, Logger } from '@nestjs/common';
import { RecordsRepository } from '../records/records.repository';
import {
  Insight,
  InsightProcessor,
  InsightSeverity,
  toDecimalValue,
  getMonthRange,
  getEndOfNextMonth,
} from '../analytics/interfaces/insight.interface';

@Injectable()
export class MonthlyComparisonProcessor implements InsightProcessor {
  private readonly logger = new Logger(MonthlyComparisonProcessor.name);

  constructor(private readonly recordsRepository: RecordsRepository) {}

  async process(userId: number): Promise<Insight[]> {
    try {
      const currentMonth = getMonthRange(0);
      const previousMonth = getMonthRange(1);

      const [currentDeposits, currentOutflows, previousDeposits, previousOutflows] =
        await Promise.all([
          this.recordsRepository.getDepositsByPeriod(userId, currentMonth.start, currentMonth.end),
          this.recordsRepository.getOutflowsByPeriod(userId, currentMonth.start, currentMonth.end),
          this.recordsRepository.getDepositsByPeriod(userId, previousMonth.start, previousMonth.end),
          this.recordsRepository.getOutflowsByPeriod(userId, previousMonth.start, previousMonth.end),
        ]);

      const sumValues = (records: { Value: unknown }[]): number =>
        records.reduce((sum, r) => sum + toDecimalValue(r.Value), 0);

      const currentDepositTotal = Math.round(sumValues(currentDeposits) * 100) / 100;
      const currentOutflowTotal = Math.round(sumValues(currentOutflows) * 100) / 100;
      const previousDepositTotal = Math.round(sumValues(previousDeposits) * 100) / 100;
      const previousOutflowTotal = Math.round(sumValues(previousOutflows) * 100) / 100;

      const currentNet = Math.round((currentDepositTotal - currentOutflowTotal) * 100) / 100;
      const previousNet = Math.round((previousDepositTotal - previousOutflowTotal) * 100) / 100;

      const calcChange = (current: number, previous: number): number => {
        if (previous === 0) return current > 0 ? 100 : 0;
        return Math.round(((current - previous) / previous) * 1000) / 10;
      };

      const depositChange = calcChange(currentDepositTotal, previousDepositTotal);
      const outflowChange = calcChange(currentOutflowTotal, previousOutflowTotal);
      const netChange = calcChange(currentNet, previousNet);

      const now = new Date();
      const currentMonthStr = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}`;
      const prevDate = new Date(now.getFullYear(), now.getMonth() - 1, 1);
      const previousMonthStr = `${prevDate.getFullYear()}-${String(prevDate.getMonth() + 1).padStart(2, '0')}`;

      let severity: InsightSeverity = 'info';
      if (outflowChange > 0) {
        if (outflowChange > 50) severity = 'critical';
        else if (outflowChange > 20) severity = 'warning';
      }

      let message: string;
      if (outflowChange > 0) {
        message = `Suas despesas em ${this.getMonthName(now)} foram ${Math.abs(outflowChange)}% maiores que em ${this.getMonthName(prevDate)}`;
      } else if (outflowChange < 0) {
        message = `Suas despesas em ${this.getMonthName(now)} foram ${Math.abs(outflowChange)}% menores que em ${this.getMonthName(prevDate)}`;
      } else {
        message = `Suas despesas se mantiveram estáveis entre ${this.getMonthName(prevDate)} e ${this.getMonthName(now)}`;
      }

      return [
        {
          userId,
          type: 'MONTHLY_COMPARISON',
          title: 'Comparativo Mensal',
          message,
          severity,
          data: {
            currentMonth: currentMonthStr,
            previousMonth: previousMonthStr,
            deposits: {
              current: currentDepositTotal,
              previous: previousDepositTotal,
              changePercent: depositChange,
            },
            outflows: {
              current: currentOutflowTotal,
              previous: previousOutflowTotal,
              changePercent: outflowChange,
            },
            netBalance: {
              current: currentNet,
              previous: previousNet,
              changePercent: netChange,
            },
          },
          period: {
            start: currentMonth.start,
            end: currentMonth.end,
          },
          expiresAt: getEndOfNextMonth(currentMonth.end),
        },
      ];
    } catch (error) {
      this.logger.error(`Erro ao processar comparativo mensal para userId=${userId}`, error);
      return [];
    }
  }

  private getMonthName(date: Date): string {
    return date.toLocaleDateString('pt-BR', { month: 'long' });
  }
}
