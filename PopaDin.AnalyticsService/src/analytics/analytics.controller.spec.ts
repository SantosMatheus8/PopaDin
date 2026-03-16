import { Test, TestingModule } from '@nestjs/testing';
import { AnalyticsController } from './analytics.controller';
import { AnalyticsService } from './analytics.service';

describe('AnalyticsController', () => {
  let controller: AnalyticsController;
  let service: jest.Mocked<AnalyticsService>;

  const mockReq = { user: { userId: 1 } };

  beforeEach(async () => {
    const mockService = {
      getInsights: jest.fn(),
      getLatestInsights: jest.fn(),
      getForecast: jest.fn(),
      processUserInsights: jest.fn(),
    };

    const module: TestingModule = await Test.createTestingModule({
      controllers: [AnalyticsController],
      providers: [{ provide: AnalyticsService, useValue: mockService }],
    }).compile();

    controller = module.get<AnalyticsController>(AnalyticsController);
    service = module.get(AnalyticsService);
  });

  describe('listInsights', () => {
    it('should return paginated insights', async () => {
      service.getInsights.mockResolvedValue({ data: [], total: 0 });

      const result = await controller.listInsights(mockReq, { page: 1, limit: 20 } as any);

      expect(service.getInsights).toHaveBeenCalledWith(1, undefined, 1, 20);
      expect(result).toEqual({ data: [], total: 0, page: 1, limit: 20 });
    });

    it('should pass type filter when provided', async () => {
      service.getInsights.mockResolvedValue({ data: [], total: 0 });

      await controller.listInsights(mockReq, {
        type: 'SPENDING_TREND',
        page: 1,
        limit: 10,
      } as any);

      expect(service.getInsights).toHaveBeenCalledWith(1, 'SPENDING_TREND', 1, 10);
    });
  });

  describe('getLatestInsights', () => {
    it('should return latest insights', async () => {
      const mockInsights = [{ type: 'SPENDING_TREND' }] as any;
      service.getLatestInsights.mockResolvedValue(mockInsights);

      const result = await controller.getLatestInsights(mockReq);

      expect(service.getLatestInsights).toHaveBeenCalledWith(1);
      expect(result).toEqual(mockInsights);
    });
  });

  describe('getForecast', () => {
    it('should return forecast insight', async () => {
      const mockForecast = { type: 'BALANCE_FORECAST' } as any;
      service.getForecast.mockResolvedValue(mockForecast);

      const result = await controller.getForecast(mockReq);

      expect(service.getForecast).toHaveBeenCalledWith(1);
      expect(result).toEqual(mockForecast);
    });

    it('should return null when no forecast exists', async () => {
      service.getForecast.mockResolvedValue(null);

      const result = await controller.getForecast(mockReq);

      expect(result).toBeNull();
    });
  });

  describe('refreshInsights', () => {
    it('should trigger processing and return success message', async () => {
      service.processUserInsights.mockResolvedValue(undefined);

      const result = await controller.refreshInsights(mockReq);

      expect(service.processUserInsights).toHaveBeenCalledWith(1);
      expect(result).toEqual({ message: 'Insights atualizados com sucesso' });
    });
  });
});
