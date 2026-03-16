import { Module } from '@nestjs/common';
import { RecordsModule } from '../records/records.module';
import { SpendingTrendProcessor } from './spending-trend.processor';
import { BalanceForecastProcessor } from './balance-forecast.processor';
import { MonthlyComparisonProcessor } from './monthly-comparison.processor';
import { AnomalyDetectionProcessor } from './anomaly-detection.processor';

@Module({
  imports: [RecordsModule],
  providers: [
    SpendingTrendProcessor,
    BalanceForecastProcessor,
    MonthlyComparisonProcessor,
    AnomalyDetectionProcessor,
  ],
  exports: [
    SpendingTrendProcessor,
    BalanceForecastProcessor,
    MonthlyComparisonProcessor,
    AnomalyDetectionProcessor,
  ],
})
export class ProcessorsModule {}
