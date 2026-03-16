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
export class AnomalyDetectionProcessor implements InsightProcessor {
  private readonly logger = new Logger(AnomalyDetectionProcessor.name);

  constructor(private readonly recordsRepository: RecordsRepository) {}

  async process(userId: number): Promise<Insight[]> {
    const insights: Insight[] = [];

    try {
      const threeMonthsAgo = getMonthRange(2);
      const currentMonth = getMonthRange(0);

      const outflows = await this.recordsRepository.getOutflowsByPeriod(
        userId,
        threeMonthsAgo.start,
        currentMonth.end,
      );

      if (outflows.length < 3) return [];

      const values = outflows.map((r) => toDecimalValue(r.Value));
      const mean = values.reduce((a, b) => a + b, 0) / values.length;
      const variance = values.reduce((sum, v) => sum + Math.pow(v - mean, 2), 0) / values.length;
      const stdDev = Math.sqrt(variance);

      if (stdDev === 0) return [];

      const threshold = mean + 2 * stdDev;

      for (const record of outflows) {
        const value = toDecimalValue(record.Value);
        if (value <= threshold) continue;

        const deviationsAbove = Math.round(((value - mean) / stdDev) * 10) / 10;
        const tagName = record.Tags?.length > 0 ? record.Tags[0].Name : 'Sem tag';

        let severity: InsightSeverity = 'warning';
        if (deviationsAbove > 3) severity = 'critical';

        insights.push({
          userId,
          type: 'ANOMALY_DETECTION',
          title: `Gasto Atípico Detectado`,
          message: `Um gasto de R$ ${value.toFixed(2).replace('.', ',')} na tag '${tagName}' está ${deviationsAbove}x acima da sua média`,
          severity,
          data: {
            recordId: record._id.toString(),
            recordName: record.Name,
            value: Math.round(value * 100) / 100,
            tagName,
            average: Math.round(mean * 100) / 100,
            standardDeviation: Math.round(stdDev * 100) / 100,
            deviationsAbove,
          },
          period: {
            start: currentMonth.start,
            end: currentMonth.end,
          },
          expiresAt: getEndOfNextMonth(currentMonth.end),
        });
      }
    } catch (error) {
      this.logger.error(`Erro ao processar detecção de anomalias para userId=${userId}`, error);
    }

    return insights;
  }
}
