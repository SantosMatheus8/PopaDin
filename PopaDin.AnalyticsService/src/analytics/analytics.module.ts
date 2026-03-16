import { Module } from '@nestjs/common';
import { MongooseModule } from '@nestjs/mongoose';
import { InsightDocument, InsightSchema } from './schemas/insight.schema';
import { AnalyticsService } from './analytics.service';
import { AnalyticsController } from './analytics.controller';
import { ProcessorsModule } from '../processors/processors.module';
import { RecordsModule } from '../records/records.module';

@Module({
  imports: [
    MongooseModule.forFeature([
      { name: InsightDocument.name, schema: InsightSchema },
    ]),
    ProcessorsModule,
    RecordsModule,
  ],
  controllers: [AnalyticsController],
  providers: [AnalyticsService],
  exports: [AnalyticsService],
})
export class AnalyticsModule {}
