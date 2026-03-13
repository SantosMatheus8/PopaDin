import { Injectable } from '@nestjs/common';
import { ServiceBusConsumer } from './servicebus.consumer';

@Injectable()
export class ServiceBusHealthService {
  constructor(private readonly consumer: ServiceBusConsumer) {}

  isHealthy(): boolean {
    return this.consumer.isConnected();
  }

  getStatus(): { serviceBus: string } {
    return {
      serviceBus: this.consumer.isConnected() ? 'connected' : 'disconnected',
    };
  }
}
