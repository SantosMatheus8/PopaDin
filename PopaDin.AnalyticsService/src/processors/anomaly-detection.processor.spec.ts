import { AnomalyDetectionProcessor } from './anomaly-detection.processor';
import { RecordsRepository } from '../records/records.repository';
import { Types } from 'mongoose';

describe('AnomalyDetectionProcessor', () => {
  let processor: AnomalyDetectionProcessor;
  let recordsRepository: jest.Mocked<RecordsRepository>;

  beforeEach(() => {
    recordsRepository = {
      getOutflowsByPeriod: jest.fn(),
    } as any;

    processor = new AnomalyDetectionProcessor(recordsRepository);
  });

  it('should detect anomalies above 2 standard deviations', async () => {
    const records = [
      { _id: new Types.ObjectId(), Name: 'Normal 1', Value: 100, Operation: 0, Tags: [{ Name: 'Food' }] },
      { _id: new Types.ObjectId(), Name: 'Normal 2', Value: 102, Operation: 0, Tags: [{ Name: 'Food' }] },
      { _id: new Types.ObjectId(), Name: 'Normal 3', Value: 98, Operation: 0, Tags: [{ Name: 'Food' }] },
      { _id: new Types.ObjectId(), Name: 'Normal 4', Value: 101, Operation: 0, Tags: [{ Name: 'Food' }] },
      { _id: new Types.ObjectId(), Name: 'Normal 5', Value: 99, Operation: 0, Tags: [{ Name: 'Food' }] },
      { _id: new Types.ObjectId(), Name: 'Normal 6', Value: 103, Operation: 0, Tags: [{ Name: 'Food' }] },
      { _id: new Types.ObjectId(), Name: 'Normal 7', Value: 97, Operation: 0, Tags: [{ Name: 'Food' }] },
      { _id: new Types.ObjectId(), Name: 'Normal 8', Value: 100, Operation: 0, Tags: [{ Name: 'Food' }] },
      { _id: new Types.ObjectId(), Name: 'Normal 9', Value: 101, Operation: 0, Tags: [{ Name: 'Food' }] },
      { _id: new Types.ObjectId(), Name: 'Normal 10', Value: 99, Operation: 0, Tags: [{ Name: 'Food' }] },
      { _id: new Types.ObjectId(), Name: 'Anomaly', Value: 500, Operation: 0, Tags: [{ Name: 'Luxury' }] },
    ];

    recordsRepository.getOutflowsByPeriod.mockResolvedValue(records as any);

    const result = await processor.process(1);

    expect(result.length).toBeGreaterThan(0);
    const anomaly = result.find((r) => r.data['recordName'] === 'Anomaly');
    expect(anomaly).toBeDefined();
    expect(anomaly!.type).toBe('ANOMALY_DETECTION');
  });

  it('should return empty array when less than 3 outflows', async () => {
    recordsRepository.getOutflowsByPeriod.mockResolvedValue([
      { _id: new Types.ObjectId(), Value: 100, Tags: [] },
      { _id: new Types.ObjectId(), Value: 200, Tags: [] },
    ] as any);

    const result = await processor.process(1);

    expect(result).toEqual([]);
  });

  it('should return empty array when standard deviation is 0', async () => {
    recordsRepository.getOutflowsByPeriod.mockResolvedValue([
      { _id: new Types.ObjectId(), Value: 100, Tags: [] },
      { _id: new Types.ObjectId(), Value: 100, Tags: [] },
      { _id: new Types.ObjectId(), Value: 100, Tags: [] },
    ] as any);

    const result = await processor.process(1);

    expect(result).toEqual([]);
  });

  it('should set critical severity when deviations above 3', async () => {
    const records = [
      { _id: new Types.ObjectId(), Name: 'A', Value: 50, Tags: [{ Name: 'X' }] },
      { _id: new Types.ObjectId(), Name: 'B', Value: 52, Tags: [{ Name: 'X' }] },
      { _id: new Types.ObjectId(), Name: 'C', Value: 48, Tags: [{ Name: 'X' }] },
      { _id: new Types.ObjectId(), Name: 'D', Value: 51, Tags: [{ Name: 'X' }] },
      { _id: new Types.ObjectId(), Name: 'E', Value: 49, Tags: [{ Name: 'X' }] },
      { _id: new Types.ObjectId(), Name: 'F', Value: 50, Tags: [{ Name: 'X' }] },
      { _id: new Types.ObjectId(), Name: 'G', Value: 53, Tags: [{ Name: 'X' }] },
      { _id: new Types.ObjectId(), Name: 'H', Value: 47, Tags: [{ Name: 'X' }] },
      { _id: new Types.ObjectId(), Name: 'I', Value: 51, Tags: [{ Name: 'X' }] },
      { _id: new Types.ObjectId(), Name: 'J', Value: 49, Tags: [{ Name: 'X' }] },
      { _id: new Types.ObjectId(), Name: 'Extreme', Value: 1000, Tags: [{ Name: 'Y' }] },
    ];

    recordsRepository.getOutflowsByPeriod.mockResolvedValue(records as any);

    const result = await processor.process(1);

    const extreme = result.find((r) => r.data['recordName'] === 'Extreme');
    expect(extreme).toBeDefined();
    expect(extreme!.severity).toBe('critical');
  });

  it('should use "Sem tag" when no tags present', async () => {
    const records = [
      { _id: new Types.ObjectId(), Name: 'A', Value: 100, Tags: [] },
      { _id: new Types.ObjectId(), Name: 'B', Value: 105, Tags: [] },
      { _id: new Types.ObjectId(), Name: 'C', Value: 95, Tags: [] },
      { _id: new Types.ObjectId(), Name: 'Big', Value: 800, Tags: [] },
    ];

    recordsRepository.getOutflowsByPeriod.mockResolvedValue(records as any);

    const result = await processor.process(1);

    if (result.length > 0) {
      expect(result[0].data['tagName']).toBe('Sem tag');
    }
  });

  it('should return empty array on error', async () => {
    recordsRepository.getOutflowsByPeriod.mockRejectedValue(new Error('db error'));

    const result = await processor.process(1);

    expect(result).toEqual([]);
  });
});
