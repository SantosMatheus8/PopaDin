import { Module } from '@nestjs/common';
import { MongooseModule } from '@nestjs/mongoose';
import { Record, RecordSchema } from './schemas/record.schema';
import {
  RecurrenceLog,
  RecurrenceLogSchema,
} from './schemas/recurrence-log.schema';
import { RecordsRepository } from './records.repository';

@Module({
  imports: [
    MongooseModule.forFeature([
      { name: Record.name, schema: RecordSchema },
      { name: RecurrenceLog.name, schema: RecurrenceLogSchema },
    ]),
  ],
  providers: [RecordsRepository],
  exports: [RecordsRepository],
})
export class RecordsModule {}
