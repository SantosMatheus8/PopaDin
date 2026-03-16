import { Prop, Schema, SchemaFactory, raw } from '@nestjs/mongoose';
import { Document } from 'mongoose';

@Schema({ _id: false })
export class InsightPeriod {
  @Prop({ required: true })
  start!: Date;

  @Prop({ required: true })
  end!: Date;
}

export const InsightPeriodSchema = SchemaFactory.createForClass(InsightPeriod);

@Schema({ collection: 'insights', timestamps: false })
export class InsightDocument {
  @Prop({ required: true })
  userId!: number;

  @Prop({ required: true })
  type!: string;

  @Prop({ required: true })
  title!: string;

  @Prop({ required: true })
  message!: string;

  @Prop({ required: true })
  severity!: string;

  @Prop({ type: raw({}), default: {} })
  data!: Record<string, unknown>;

  @Prop({ type: InsightPeriodSchema, required: true })
  period!: InsightPeriod;

  @Prop({ required: true, index: true })
  expiresAt!: Date;

  @Prop({ default: () => new Date() })
  createdAt!: Date;
}

export type InsightDoc = InsightDocument & Document;

export const InsightSchema = SchemaFactory.createForClass(InsightDocument);

InsightSchema.index({ expiresAt: 1 }, { expireAfterSeconds: 0 });

InsightSchema.index({ userId: 1, type: 1, 'period.start': 1 });
