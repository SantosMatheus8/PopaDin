import { Injectable } from '@nestjs/common';
import { InjectModel } from '@nestjs/mongoose';
import { Model, Types } from 'mongoose';
import { Record, RecordDocument } from './schemas/record.schema';

export interface LeanRecord {
  _id: Types.ObjectId;
  Name: string;
  Value: Types.Decimal128 | number;
  Operation: number;
  Frequency: number;
  ReferenceDate: Date;
  CreatedAt: Date;
  UpdatedAt: Date;
  Tags: { OriginalTagId: number; Name: string; Color: string; TagType: number; Description: string }[];
  UserId: number;
  InstallmentGroupId: string | null;
  InstallmentIndex: number | null;
  InstallmentTotal: number | null;
  RecurrenceEndDate: Date | null;
}

export interface TagSpending {
  tagId: number;
  tagName: string;
  total: number;
}

@Injectable()
export class RecordsRepository {
  constructor(
    @InjectModel(Record.name) private readonly recordModel: Model<RecordDocument>,
  ) {}

  async getOutflowsByPeriod(userId: number, startDate: Date, endDate: Date): Promise<LeanRecord[]> {
    return this.recordModel
      .find({
        UserId: userId,
        Operation: 0,
        ReferenceDate: { $gte: startDate, $lte: endDate },
      })
      .lean<LeanRecord[]>()
      .exec();
  }

  async getDepositsByPeriod(userId: number, startDate: Date, endDate: Date): Promise<LeanRecord[]> {
    return this.recordModel
      .find({
        UserId: userId,
        Operation: 1,
        ReferenceDate: { $gte: startDate, $lte: endDate },
      })
      .lean<LeanRecord[]>()
      .exec();
  }

  async getRecurringRecords(userId: number): Promise<LeanRecord[]> {
    return this.recordModel
      .find({
        UserId: userId,
        Frequency: { $ne: 5 },
        InstallmentGroupId: null,
      })
      .lean<LeanRecord[]>()
      .exec();
  }

  async getRecordsByTagInPeriod(userId: number, startDate: Date, endDate: Date): Promise<TagSpending[]> {
    const result = await this.recordModel.aggregate([
      {
        $match: {
          UserId: userId,
          Operation: 0,
          ReferenceDate: { $gte: startDate, $lte: endDate },
        },
      },
      { $unwind: '$Tags' },
      {
        $group: {
          _id: { tagId: '$Tags.OriginalTagId', tagName: '$Tags.Name' },
          total: { $sum: { $toDouble: '$Value' } },
        },
      },
      {
        $project: {
          _id: 0,
          tagId: '$_id.tagId',
          tagName: '$_id.tagName',
          total: 1,
        },
      },
      { $sort: { total: -1 } },
    ]);

    return result;
  }

  async getAllRecordsInPeriod(userId: number, startDate: Date, endDate: Date): Promise<LeanRecord[]> {
    return this.recordModel
      .find({
        UserId: userId,
        ReferenceDate: { $gte: startDate, $lte: endDate },
      })
      .lean<LeanRecord[]>()
      .exec();
  }

  async getDistinctUserIdsWithRecentActivity(since: Date): Promise<number[]> {
    const result = await this.recordModel.distinct('UserId', {
      CreatedAt: { $gte: since },
    });
    return result as number[];
  }
}
