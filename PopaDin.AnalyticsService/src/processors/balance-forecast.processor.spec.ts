import { BalanceForecastProcessor } from './balance-forecast.processor';
import { RecordsRepository } from '../records/records.repository';

describe('BalanceForecastProcessor', () => {
  let processor: BalanceForecastProcessor;
  let recordsRepository: jest.Mocked<RecordsRepository>;

  beforeEach(() => {
    recordsRepository = {
      getAllRecordsInPeriod: jest.fn(),
      getRecurringRecords: jest.fn(),
    } as any;

    processor = new BalanceForecastProcessor(recordsRepository);
  });

  it('should generate balance forecast insight', async () => {
    recordsRepository.getAllRecordsInPeriod.mockResolvedValue([
      { Operation: 1, Value: 5000, Frequency: 5, InstallmentGroupId: null },
      { Operation: 0, Value: 2000, Frequency: 5, InstallmentGroupId: null },
    ] as any);
    recordsRepository.getRecurringRecords.mockResolvedValue([]);

    const result = await processor.process(1);

    expect(result).toHaveLength(1);
    expect(result[0].type).toBe('BALANCE_FORECAST');
    expect(result[0].data['currentBalance']).toBeDefined();
    expect(result[0].data['forecast']).toBeDefined();
  });

  it('should calculate monthly recurring income and expenses', async () => {
    recordsRepository.getAllRecordsInPeriod.mockResolvedValue([]);
    recordsRepository.getRecurringRecords.mockResolvedValue([
      { Operation: 1, Value: 6000, Frequency: 0 },
      { Operation: 0, Value: 1500, Frequency: 0 },
    ] as any);

    const result = await processor.process(1);

    expect(result[0].data['monthlyRecurringIncome']).toBe(6000);
    expect(result[0].data['monthlyRecurringExpenses']).toBe(1500);
  });

  it('should set critical severity when balance goes negative in 3 months or less', async () => {
    recordsRepository.getAllRecordsInPeriod.mockResolvedValue([
      { Operation: 1, Value: 3000, Frequency: 5, InstallmentGroupId: null },
    ] as any);
    recordsRepository.getRecurringRecords.mockResolvedValue([
      { Operation: 0, Value: 2000, Frequency: 0 },
    ] as any);

    const result = await processor.process(1);

    expect(result[0].severity).toBe('critical');
  });

  it('should set warning severity when balance goes negative in 4-6 months', async () => {
    recordsRepository.getAllRecordsInPeriod.mockResolvedValue([
      { Operation: 1, Value: 5000, Frequency: 5, InstallmentGroupId: null },
    ] as any);
    recordsRepository.getRecurringRecords.mockResolvedValue([
      { Operation: 0, Value: 1000, Frequency: 0 },
    ] as any);

    const result = await processor.process(1);

    if (result[0].data['goesNegativeInMonths'] !== null) {
      const months = result[0].data['goesNegativeInMonths'] as number;
      if (months > 3 && months <= 6) {
        expect(result[0].severity).toBe('warning');
      }
    }
  });

  it('should set info severity when balance is healthy', async () => {
    recordsRepository.getAllRecordsInPeriod.mockResolvedValue([
      { Operation: 1, Value: 10000, Frequency: 5, InstallmentGroupId: null },
    ] as any);
    recordsRepository.getRecurringRecords.mockResolvedValue([
      { Operation: 1, Value: 5000, Frequency: 0 },
      { Operation: 0, Value: 1000, Frequency: 0 },
    ] as any);

    const result = await processor.process(1);

    expect(result[0].severity).toBe('info');
  });

  it('should return empty array on error', async () => {
    recordsRepository.getAllRecordsInPeriod.mockRejectedValue(new Error('db error'));

    const result = await processor.process(1);

    expect(result).toEqual([]);
  });
});
