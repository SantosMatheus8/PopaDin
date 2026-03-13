import { Module } from '@nestjs/common';
import { NotificationsModule } from '../notifications/notifications.module';
import { ServiceBusConsumer } from './servicebus.consumer';
import { ServiceBusHealthService } from './servicebus.health';

@Module({
  imports: [NotificationsModule],
  providers: [ServiceBusConsumer, ServiceBusHealthService],
  exports: [ServiceBusHealthService],
})
export class ServiceBusModule {}
