import { Prop, Schema, SchemaFactory } from '@nestjs/mongoose';
import { Document, Types } from 'mongoose';

@Schema({ collection: 'recurrence_logs', timestamps: false })
export class RecurrenceLog {
  _id!: Types.ObjectId;

  @Prop()
  SourceRecordId!: string;

  @Prop()
  GeneratedRecordId!: string;

  @Prop()
  OccurrenceDate!: Date;

  @Prop()
  ProcessedAt!: Date;
}

export type RecurrenceLogDocument = RecurrenceLog & Document;
export const RecurrenceLogSchema = SchemaFactory.createForClass(RecurrenceLog);
