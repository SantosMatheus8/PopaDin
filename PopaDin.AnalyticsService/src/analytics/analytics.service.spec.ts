import { Test, TestingModule } from '@nestjs/testing';
import { getModelToken } from '@nestjs/mongoose';
import { ConfigService } from '@nestjs/config';
import { AnalyticsService } from './analytics.service';
import { InsightDocument } from './schemas/insight.schema';
import { SpendingTrendProcessor } from '../processors/spending-trend.processor';
import { BalanceForecastProcessor } from '../processors/balance-forecast.processor';
import { MonthlyComparisonProcessor } from '../processors/monthly-comparison.processor';
import { AnomalyDetectionProcessor } from '../processors/anomaly-detection.processor';
import { RecordsRepository } from '../records/records.repository';

describe('AnalyticsService', () => {
  let service: AnalyticsService;
  let mockInsightModel: any;
  let spendingTrendProcessor: jest.Mocked<SpendingTrendProcessor>;
  let balanceForecastProcessor: jest.Mocked<BalanceForecastProcessor>;
  let monthlyComparisonProcessor: jest.Mocked<MonthlyComparisonProcessor>;
  let anomalyDetectionProcessor: jest.Mocked<AnomalyDetectionProcessor>;
  let recordsRepository: jest.Mocked<RecordsRepository>;

  beforeEach(async () => {
    mockInsightModel = {
      findOneAndUpdate: jest.fn().mockResolvedValue(null),
      find: jest.fn().mockReturnValue({
        sort: jest.fn().mockReturnValue({
          skip: jest.fn().mockReturnValue({
            limit: jest.fn().mockReturnValue({
              lean: jest.fn().mockReturnValue({
                exec: jest.fn().mockResolvedValue([]),
              }),
            }),
          }),
        }),
      }),
      findOne: jest.fn().mockReturnValue({
        sort: jest.fn().mockReturnValue({
          lean: jest.fn().mockReturnValue({
            exec: jest.fn().mockResolvedValue(null),
          }),
        }),
      }),
      countDocuments: jest.fn().mockResolvedValue(0),
    };

    const module: TestingModule = await Test.createTestingModule({
      providers: [
        AnalyticsService,
        { provide: getModelToken(InsightDocument.name), useValue: mockInsightModel },
        { provide: SpendingTrendProcessor, useValue: { process: jest.fn().mockResolvedValue([]) } },
        { provide: BalanceForecastProcessor, useValue: { process: jest.fn().mockResolvedValue([]) } },
        { provide: MonthlyComparisonProcessor, useValue: { process: jest.fn().mockResolvedValue([]) } },
        { provide: AnomalyDetectionProcessor, useValue: { process: jest.fn().mockResolvedValue([]) } },
        { provide: RecordsRepository, useValue: { getDistinctUserIdsWithRecentActivity: jest.fn().mockResolvedValue([]) } },
        { provide: ConfigService, useValue: { get: jest.fn() } },
      ],
    }).compile();

    service = module.get<AnalyticsService>(AnalyticsService);
    spendingTrendProcessor = module.get(SpendingTrendProcessor);
    balanceForecastProcessor = module.get(BalanceForecastProcessor);
    monthlyComparisonProcessor = module.get(MonthlyComparisonProcessor);
    anomalyDetectionProcessor = module.get(AnomalyDetectionProcessor);
    recordsRepository = module.get(RecordsRepository);
  });

  describe('processUserInsights', () => {
    it('should call all processors and upsert results', async () => {
      const mockInsight = {
        userId: 1,
        type: 'SPENDING_TREND',
        title: 'Test',
        message: 'Test',
        severity: 'info',
        data: {},
        period: { start: new Date(), end: new Date() },
        expiresAt: new Date(),
      };

      (spendingTrendProcessor.process as jest.Mock).mockResolvedValue([mockInsight]);

      await service.processUserInsights(1);

      expect(spendingTrendProcessor.process).toHaveBeenCalledWith(1);
      expect(balanceForecastProcessor.process).toHaveBeenCalledWith(1);
      expect(monthlyComparisonProcessor.process).toHaveBeenCalledWith(1);
      expect(anomalyDetectionProcessor.process).toHaveBeenCalledWith(1);
      expect(mockInsightModel.findOneAndUpdate).toHaveBeenCalled();
    });

    it('should not throw on processor error', async () => {
      (spendingTrendProcessor.process as jest.Mock).mockRejectedValue(new Error('fail'));

      await expect(service.processUserInsights(1)).resolves.not.toThrow();
    });
  });

  describe('getInsights', () => {
    it('should return paginated data', async () => {
      const result = await service.getInsights(1, undefined, 1, 20);

      expect(result).toEqual({ data: [], total: 0 });
    });

    it('should filter by type when provided', async () => {
      await service.getInsights(1, 'BALANCE_FORECAST', 1, 10);

      expect(mockInsightModel.find).toHaveBeenCalledWith({ userId: 1, type: 'BALANCE_FORECAST' });
    });
  });

  describe('getLatestInsights', () => {
    it('should query each insight type', async () => {
      const result = await service.getLatestInsights(1);

      expect(mockInsightModel.findOne).toHaveBeenCalledTimes(4);
      expect(result).toEqual([]);
    });
  });

  describe('getForecast', () => {
    it('should query BALANCE_FORECAST type', async () => {
      const result = await service.getForecast(1);

      expect(mockInsightModel.findOne).toHaveBeenCalledWith({ userId: 1, type: 'BALANCE_FORECAST' });
      expect(result).toBeNull();
    });
  });

  describe('handleCron', () => {
    it('should process insights for users with recent activity', async () => {
      (recordsRepository.getDistinctUserIdsWithRecentActivity as jest.Mock).mockResolvedValue([1, 2]);

      await service.handleCron();

      expect(recordsRepository.getDistinctUserIdsWithRecentActivity).toHaveBeenCalled();
      expect(spendingTrendProcessor.process).toHaveBeenCalledWith(1);
      expect(spendingTrendProcessor.process).toHaveBeenCalledWith(2);
    });

    it('should not throw on cron error', async () => {
      (recordsRepository.getDistinctUserIdsWithRecentActivity as jest.Mock).mockRejectedValue(new Error('db error'));

      await expect(service.handleCron()).resolves.not.toThrow();
    });
  });
});
