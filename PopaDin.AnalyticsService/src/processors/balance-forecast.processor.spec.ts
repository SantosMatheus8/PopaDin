import { BalanceForecastProcessor } from './balance-forecast.processor';
import { RecordsRepository } from '../records/records.repository';

describe('BalanceForecastProcessor', () => {
  let processor: BalanceForecastProcessor;
  let recordsRepository: jest.Mocked<RecordsRepository>;

  beforeEach(() => {
    recordsRepository = {
      getAllRecordsInPeriod: jest.fn(),
      getRecurringRecords: jest.fn(),
      getCumulativeBalance: jest.fn().mockResolvedValue(0),
      getMaterializedOccurrencesUpTo: jest.fn().mockResolvedValue(new Set()),
    } as any;

    processor = new BalanceForecastProcessor(recordsRepository);
  });

  it('should generate balance forecast insight', async () => {
    recordsRepository.getAllRecordsInPeriod.mockResolvedValue([
      { Operation: 1, Value: 5000, Frequency: 5, InstallmentGroupId: null },
      { Operation: 0, Value: 2000, Frequency: 5, InstallmentGroupId: null },
    ] as any);
    recordsRepository.getRecurringRecords.mockResolvedValue([]);
    recordsRepository.getCumulativeBalance.mockResolvedValue(3000);

    const result = await processor.process(1);

    expect(result).toHaveLength(1);
    expect(result[0].type).toBe('BALANCE_FORECAST');
    expect(result[0].data['currentBalance']).toBe(3000);
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
    recordsRepository.getAllRecordsInPeriod.mockResolvedValue([]);
    recordsRepository.getRecurringRecords.mockResolvedValue([
      { Operation: 0, Value: 2000, Frequency: 0 },
    ] as any);
    recordsRepository.getCumulativeBalance.mockResolvedValue(3000);

    const result = await processor.process(1);

    expect(result[0].severity).toBe('critical');
  });

  it('should set warning severity when balance goes negative in 4-6 months', async () => {
    recordsRepository.getAllRecordsInPeriod.mockResolvedValue([]);
    recordsRepository.getRecurringRecords.mockResolvedValue([
      { Operation: 0, Value: 1000, Frequency: 0 },
    ] as any);
    recordsRepository.getCumulativeBalance.mockResolvedValue(5000);

    const result = await processor.process(1);

    expect(result[0].severity).toBe('warning');
    expect(result[0].data['goesNegativeInMonths']).toBe(5);
  });

  it('should set info severity when balance is healthy', async () => {
    recordsRepository.getAllRecordsInPeriod.mockResolvedValue([]);
    recordsRepository.getRecurringRecords.mockResolvedValue([
      { Operation: 1, Value: 5000, Frequency: 0 },
      { Operation: 0, Value: 1000, Frequency: 0 },
    ] as any);
    recordsRepository.getCumulativeBalance.mockResolvedValue(10000);

    const result = await processor.process(1);

    expect(result[0].severity).toBe('info');
  });

  it('should set critical severity when balance is already negative', async () => {
    recordsRepository.getAllRecordsInPeriod.mockResolvedValue([]);
    recordsRepository.getRecurringRecords.mockResolvedValue([
      { Operation: 0, Value: 100, Frequency: 0 },
    ] as any);
    recordsRepository.getCumulativeBalance.mockResolvedValue(-500);

    const result = await processor.process(1);

    expect(result[0].severity).toBe('critical');
    expect(result[0].data['currentBalance']).toBe(-500);
    expect(result[0].data['goesNegativeInMonths']).toBe(0);
  });

  it('should include unmaterialized recurring impact in balance', async () => {
    const baseDate = new Date();
    baseDate.setDate(15);
    // Set to 2 months ago so there are unmaterialized occurrences
    baseDate.setMonth(baseDate.getMonth() - 2);

    const recordId = '507f1f77bcf86cd799439011';

    recordsRepository.getAllRecordsInPeriod.mockResolvedValue([]);
    recordsRepository.getRecurringRecords.mockResolvedValue([
      {
        _id: { toString: () => recordId },
        Operation: 0,
        Value: 100,
        Frequency: 0, // Monthly
        ReferenceDate: baseDate,
        RecurrenceEndDate: null,
      },
    ] as any);
    recordsRepository.getCumulativeBalance.mockResolvedValue(-300);
    // Only one occurrence materialized out of 3 expected
    const materializedSet = new Set<string>();
    const matDate = new Date(baseDate);
    materializedSet.add(`${recordId}|${matDate.toISOString().split('T')[0]}`);
    recordsRepository.getMaterializedOccurrencesUpTo.mockResolvedValue(materializedSet);

    const result = await processor.process(1);

    // Balance should be cumulativeBalance + unmaterialized impact
    // -300 + (-100 * 2 unmaterialized occurrences) = -500
    expect(result[0].data['currentBalance']).toBe(-500);
  });

  it('should return empty array on error', async () => {
    recordsRepository.getAllRecordsInPeriod.mockRejectedValue(new Error('db error'));

    const result = await processor.process(1);

    expect(result).toEqual([]);
  });
});
