import { Injectable, Logger } from '@nestjs/common';
import { InjectModel } from '@nestjs/mongoose';
import { Model, Types } from 'mongoose';
import { Cron } from '@nestjs/schedule';
import { ConfigService } from '@nestjs/config';
import { InsightDocument } from './schemas/insight.schema';
import { SpendingTrendProcessor } from '../processors/spending-trend.processor';
import { BalanceForecastProcessor } from '../processors/balance-forecast.processor';
import { MonthlyComparisonProcessor } from '../processors/monthly-comparison.processor';
import { AnomalyDetectionProcessor } from '../processors/anomaly-detection.processor';
import { RecordsRepository } from '../records/records.repository';
import { Insight } from './interfaces/insight.interface';

export interface LeanInsight {
  _id: Types.ObjectId;
  userId: number;
  type: string;
  title: string;
  message: string;
  severity: string;
  data: Record<string, unknown>;
  period: { start: Date; end: Date };
  expiresAt: Date;
  createdAt: Date;
}

@Injectable()
export class AnalyticsService {
  private readonly logger = new Logger(AnalyticsService.name);

  constructor(
    @InjectModel(InsightDocument.name) private readonly insightModel: Model<InsightDocument>,
    private readonly spendingTrendProcessor: SpendingTrendProcessor,
    private readonly balanceForecastProcessor: BalanceForecastProcessor,
    private readonly monthlyComparisonProcessor: MonthlyComparisonProcessor,
    private readonly anomalyDetectionProcessor: AnomalyDetectionProcessor,
    private readonly recordsRepository: RecordsRepository,
    private readonly configService: ConfigService,
  ) {}

  async processUserInsights(userId: number): Promise<void> {
    this.logger.log(`Processando insights para userId=${userId}`);

    try {
      const [spendingTrends, balanceForecast, monthlyComparison, anomalies] =
        await Promise.all([
          this.spendingTrendProcessor.process(userId),
          this.balanceForecastProcessor.process(userId),
          this.monthlyComparisonProcessor.process(userId),
          this.anomalyDetectionProcessor.process(userId),
        ]);

      const allInsights: Insight[] = [
        ...spendingTrends,
        ...balanceForecast,
        ...monthlyComparison,
        ...anomalies,
      ];

      for (const insight of allInsights) {
        await this.upsertInsight(insight);
      }

      this.logger.log(`${allInsights.length} insights processados para userId=${userId}`);
    } catch (error) {
      this.logger.error(`Erro ao processar insights para userId=${userId}`, error);
    }
  }

  private async upsertInsight(insight: Insight): Promise<void> {
    const filter: Record<string, unknown> = {
      userId: insight.userId,
      type: insight.type,
      'period.start': insight.period.start,
    };

    if (insight.type === 'SPENDING_TREND' && insight.data['tagId'] !== undefined) {
      filter['data.tagId'] = insight.data['tagId'];
    }
    if (insight.type === 'ANOMALY_DETECTION' && insight.data['recordId'] !== undefined) {
      filter['data.recordId'] = insight.data['recordId'];
    }

    await this.insightModel.findOneAndUpdate(
      filter,
      {
        $set: {
          ...insight,
          createdAt: new Date(),
        },
      },
      { upsert: true },
    );
  }

  async getInsights(
    userId: number,
    type?: string,
    page: number = 1,
    limit: number = 20,
  ): Promise<{ data: LeanInsight[]; total: number }> {
    const filter: Record<string, unknown> = { userId };
    if (type) filter.type = type;

    const [data, total] = await Promise.all([
      this.insightModel
        .find(filter)
        .sort({ createdAt: -1 })
        .skip((page - 1) * limit)
        .limit(limit)
        .lean<LeanInsight[]>()
        .exec(),
      this.insightModel.countDocuments(filter),
    ]);

    return { data, total };
  }

  async getLatestInsights(userId: number): Promise<LeanInsight[]> {
    const types = ['SPENDING_TREND', 'BALANCE_FORECAST', 'MONTHLY_COMPARISON', 'ANOMALY_DETECTION'];
    const results: LeanInsight[] = [];

    for (const type of types) {
      const latest = await this.insightModel
        .findOne({ userId, type })
        .sort({ createdAt: -1 })
        .lean<LeanInsight>()
        .exec();

      if (latest) results.push(latest);
    }

    return results;
  }

  async getForecast(userId: number): Promise<LeanInsight | null> {
    return this.insightModel
      .findOne({ userId, type: 'BALANCE_FORECAST' })
      .sort({ createdAt: -1 })
      .lean<LeanInsight>()
      .exec();
  }

  @Cron(process.env.CRON_SCHEDULE ?? '0 */6 * * *')
  async handleCron(): Promise<void> {
    this.logger.log('Cron: iniciando processamento de insights');

    try {
      const since = new Date();
      since.setHours(since.getHours() - 24);

      const userIds = await this.recordsRepository.getDistinctUserIdsWithRecentActivity(since);

      this.logger.log(`Cron: ${userIds.length} usuários com atividade recente`);

      for (const userId of userIds) {
        await this.processUserInsights(userId);
      }

      this.logger.log('Cron: processamento concluído');
    } catch (error) {
      this.logger.error('Cron: erro no processamento', error);
    }
  }
}
