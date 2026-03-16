import { Prop, Schema, SchemaFactory, raw } from '@nestjs/mongoose';
import { Document, Types } from 'mongoose';

@Schema({ _id: false })
export class RecordTag {
  @Prop()
  OriginalTagId!: number;

  @Prop()
  Name!: string;

  @Prop()
  Color!: string;

  @Prop()
  TagType!: number;

  @Prop()
  Description!: string;
}

export const RecordTagSchema = SchemaFactory.createForClass(RecordTag);

@Schema({ collection: 'records', timestamps: false })
export class Record {
  _id!: Types.ObjectId;

  @Prop()
  Name!: string;

  @Prop(raw({ type: 'Decimal128' }))
  Value!: Types.Decimal128;

  @Prop()
  Operation!: number;

  @Prop()
  Frequency!: number;

  @Prop()
  ReferenceDate!: Date;

  @Prop()
  CreatedAt!: Date;

  @Prop()
  UpdatedAt!: Date;

  @Prop({ type: [RecordTagSchema], default: [] })
  Tags!: RecordTag[];

  @Prop()
  UserId!: number;

  @Prop({ type: String, default: null })
  InstallmentGroupId!: string | null;

  @Prop({ type: Number, default: null })
  InstallmentIndex!: number | null;

  @Prop({ type: Number, default: null })
  InstallmentTotal!: number | null;

  @Prop({ type: Date, default: null })
  RecurrenceEndDate!: Date | null;
}

export type RecordDocument = Record & Document;
export const RecordSchema = SchemaFactory.createForClass(Record);
