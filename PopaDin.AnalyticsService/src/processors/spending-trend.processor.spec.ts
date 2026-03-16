import { SpendingTrendProcessor } from './spending-trend.processor';
import { RecordsRepository } from '../records/records.repository';

describe('SpendingTrendProcessor', () => {
  let processor: SpendingTrendProcessor;
  let recordsRepository: jest.Mocked<RecordsRepository>;

  beforeEach(() => {
    recordsRepository = {
      getRecordsByTagInPeriod: jest.fn(),
    } as any;

    processor = new SpendingTrendProcessor(recordsRepository);
  });

  it('should detect spending increase above 20%', async () => {
    recordsRepository.getRecordsByTagInPeriod
      .mockResolvedValueOnce([{ tagId: 1, tagName: 'Food', total: 600 }])
      .mockResolvedValueOnce([{ tagId: 1, tagName: 'Food', total: 400 }]);

    const result = await processor.process(1);

    expect(result).toHaveLength(1);
    expect(result[0].type).toBe('SPENDING_TREND');
    expect(result[0].data['direction']).toBe('up');
    expect(result[0].data['changePercent']).toBe(50);
  });

  it('should detect spending decrease above 20%', async () => {
    recordsRepository.getRecordsByTagInPeriod
      .mockResolvedValueOnce([{ tagId: 1, tagName: 'Food', total: 300 }])
      .mockResolvedValueOnce([{ tagId: 1, tagName: 'Food', total: 500 }]);

    const result = await processor.process(1);

    expect(result).toHaveLength(1);
    expect(result[0].data['direction']).toBe('down');
    expect(result[0].severity).toBe('info');
  });

  it('should set critical severity for increase above 50%', async () => {
    recordsRepository.getRecordsByTagInPeriod
      .mockResolvedValueOnce([{ tagId: 1, tagName: 'Food', total: 800 }])
      .mockResolvedValueOnce([{ tagId: 1, tagName: 'Food', total: 400 }]);

    const result = await processor.process(1);

    expect(result[0].severity).toBe('critical');
  });

  it('should set warning severity for increase between 20-50%', async () => {
    recordsRepository.getRecordsByTagInPeriod
      .mockResolvedValueOnce([{ tagId: 1, tagName: 'Food', total: 550 }])
      .mockResolvedValueOnce([{ tagId: 1, tagName: 'Food', total: 400 }]);

    const result = await processor.process(1);

    expect(result[0].severity).toBe('warning');
  });

  it('should ignore changes below 20%', async () => {
    recordsRepository.getRecordsByTagInPeriod
      .mockResolvedValueOnce([{ tagId: 1, tagName: 'Food', total: 410 }])
      .mockResolvedValueOnce([{ tagId: 1, tagName: 'Food', total: 400 }]);

    const result = await processor.process(1);

    expect(result).toHaveLength(0);
  });

  it('should skip tags with zero previous spending', async () => {
    recordsRepository.getRecordsByTagInPeriod
      .mockResolvedValueOnce([{ tagId: 1, tagName: 'Food', total: 500 }])
      .mockResolvedValueOnce([{ tagId: 1, tagName: 'Food', total: 0 }]);

    const result = await processor.process(1);

    expect(result).toHaveLength(0);
  });

  it('should skip tags not present in previous month', async () => {
    recordsRepository.getRecordsByTagInPeriod
      .mockResolvedValueOnce([{ tagId: 1, tagName: 'Food', total: 500 }])
      .mockResolvedValueOnce([]);

    const result = await processor.process(1);

    expect(result).toHaveLength(0);
  });

  it('should return empty array on error', async () => {
    recordsRepository.getRecordsByTagInPeriod.mockRejectedValue(new Error('db error'));

    const result = await processor.process(1);

    expect(result).toEqual([]);
  });
});
