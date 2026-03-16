import { MonthlyComparisonProcessor } from './monthly-comparison.processor';
import { RecordsRepository } from '../records/records.repository';

describe('MonthlyComparisonProcessor', () => {
  let processor: MonthlyComparisonProcessor;
  let recordsRepository: jest.Mocked<RecordsRepository>;

  beforeEach(() => {
    recordsRepository = {
      getDepositsByPeriod: jest.fn(),
      getOutflowsByPeriod: jest.fn(),
    } as any;

    processor = new MonthlyComparisonProcessor(recordsRepository);
  });

  it('should generate monthly comparison insight', async () => {
    recordsRepository.getDepositsByPeriod
      .mockResolvedValueOnce([{ Value: 5000 }] as any)
      .mockResolvedValueOnce([{ Value: 4000 }] as any);
    recordsRepository.getOutflowsByPeriod
      .mockResolvedValueOnce([{ Value: 3000 }] as any)
      .mockResolvedValueOnce([{ Value: 2000 }] as any);

    const result = await processor.process(1);

    expect(result).toHaveLength(1);
    expect(result[0].type).toBe('MONTHLY_COMPARISON');
    expect(result[0].data['deposits']).toBeDefined();
    expect(result[0].data['outflows']).toBeDefined();
    expect(result[0].data['netBalance']).toBeDefined();
  });

  it('should set critical severity when outflow increase > 50%', async () => {
    recordsRepository.getDepositsByPeriod
      .mockResolvedValue([{ Value: 5000 }] as any);
    recordsRepository.getOutflowsByPeriod
      .mockResolvedValueOnce([{ Value: 4500 }] as any)
      .mockResolvedValueOnce([{ Value: 2000 }] as any);

    const result = await processor.process(1);

    expect(result[0].severity).toBe('critical');
  });

  it('should set warning severity when outflow increase 20-50%', async () => {
    recordsRepository.getDepositsByPeriod
      .mockResolvedValue([{ Value: 5000 }] as any);
    recordsRepository.getOutflowsByPeriod
      .mockResolvedValueOnce([{ Value: 3000 }] as any)
      .mockResolvedValueOnce([{ Value: 2000 }] as any);

    const result = await processor.process(1);

    expect(result[0].severity).toBe('warning');
  });

  it('should set info severity when outflows decrease', async () => {
    recordsRepository.getDepositsByPeriod
      .mockResolvedValue([{ Value: 5000 }] as any);
    recordsRepository.getOutflowsByPeriod
      .mockResolvedValueOnce([{ Value: 1500 }] as any)
      .mockResolvedValueOnce([{ Value: 3000 }] as any);

    const result = await processor.process(1);

    expect(result[0].severity).toBe('info');
  });

  it('should handle zero previous outflows', async () => {
    recordsRepository.getDepositsByPeriod
      .mockResolvedValue([] as any);
    recordsRepository.getOutflowsByPeriod
      .mockResolvedValueOnce([{ Value: 1000 }] as any)
      .mockResolvedValueOnce([] as any);

    const result = await processor.process(1);

    expect(result).toHaveLength(1);
  });

  it('should return empty array on error', async () => {
    recordsRepository.getDepositsByPeriod.mockRejectedValue(new Error('fail'));

    const result = await processor.process(1);

    expect(result).toEqual([]);
  });
});
