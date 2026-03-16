import { Injectable, Logger } from '@nestjs/common';
import { RecordsRepository } from '../records/records.repository';
import {
  Insight,
  InsightProcessor,
  InsightSeverity,
  getMonthRange,
  getEndOfNextMonth,
} from '../analytics/interfaces/insight.interface';

@Injectable()
export class SpendingTrendProcessor implements InsightProcessor {
  private readonly logger = new Logger(SpendingTrendProcessor.name);

  constructor(private readonly recordsRepository: RecordsRepository) {}

  async process(userId: number): Promise<Insight[]> {
    const insights: Insight[] = [];

    try {
      const currentMonth = getMonthRange(0);
      const previousMonth = getMonthRange(1);

      const [currentByTag, previousByTag] = await Promise.all([
        this.recordsRepository.getRecordsByTagInPeriod(userId, currentMonth.start, currentMonth.end),
        this.recordsRepository.getRecordsByTagInPeriod(userId, previousMonth.start, previousMonth.end),
      ]);

      const previousMap = new Map(previousByTag.map((t) => [t.tagId, t]));

      for (const current of currentByTag) {
        const previous = previousMap.get(current.tagId);
        if (!previous || previous.total === 0) continue;

        const changePercent = ((current.total - previous.total) / previous.total) * 100;
        const roundedChange = Math.round(changePercent * 10) / 10;

        if (Math.abs(roundedChange) < 20) continue;

        const direction = roundedChange > 0 ? 'up' : 'down';
        const absChange = Math.abs(roundedChange);

        let severity: InsightSeverity = 'info';
        if (direction === 'up') {
          // Gastos aumentando é negativo
          if (absChange > 50) severity = 'critical';
          else if (absChange > 20) severity = 'warning';
        }
        // Gastos diminuindo é positivo — sempre 'info'

        const directionText = direction === 'up' ? 'aumentaram' : 'diminuíram';
        const title =
          direction === 'up'
            ? `Gastos com ${current.tagName} em Alta`
            : `Gastos com ${current.tagName} em Queda`;

        insights.push({
          userId,
          type: 'SPENDING_TREND',
          title,
          message: `Seus gastos com '${current.tagName}' ${directionText} ${absChange}% em relação ao mês anterior`,
          severity,
          data: {
            tagId: current.tagId,
            tagName: current.tagName,
            currentMonth: Math.round(current.total * 100) / 100,
            previousMonth: Math.round(previous.total * 100) / 100,
            changePercent: roundedChange,
            direction,
          },
          period: {
            start: currentMonth.start,
            end: currentMonth.end,
          },
          expiresAt: getEndOfNextMonth(currentMonth.end),
        });
      }
    } catch (error) {
      this.logger.error(`Erro ao processar tendência de gastos para userId=${userId}`, error);
    }

    return insights;
  }
}
