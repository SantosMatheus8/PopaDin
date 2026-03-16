import { Test, TestingModule } from '@nestjs/testing';
import { ConfigService } from '@nestjs/config';
import { ServiceBusConsumer } from './servicebus.consumer';
import { NotificationsService } from '../notifications/notifications.service';
import { NotificationsGateway } from '../notifications/notifications.gateway';

describe('ServiceBusConsumer', () => {
  let consumer: ServiceBusConsumer;
  let configService: jest.Mocked<ConfigService>;
  let notificationsService: jest.Mocked<NotificationsService>;
  let notificationsGateway: jest.Mocked<NotificationsGateway>;

  beforeEach(async () => {
    const mockConfigService = {
      get: jest.fn().mockImplementation((key: string, defaultValue?: string) => {
        if (key === 'serviceBus.connectionString') return '';
        if (key === 'serviceBus.queueName') return defaultValue ?? 'notifications';
        return defaultValue;
      }),
    };

    const mockNotificationsService = {
      create: jest.fn().mockResolvedValue({ _id: 'n1', userId: 1 }),
    };

    const mockNotificationsGateway = {
      sendNotification: jest.fn().mockResolvedValue(undefined),
    };

    const module: TestingModule = await Test.createTestingModule({
      providers: [
        ServiceBusConsumer,
        { provide: ConfigService, useValue: mockConfigService },
        { provide: NotificationsService, useValue: mockNotificationsService },
        { provide: NotificationsGateway, useValue: mockNotificationsGateway },
      ],
    }).compile();

    consumer = module.get<ServiceBusConsumer>(ServiceBusConsumer);
    configService = module.get(ConfigService);
    notificationsService = module.get(NotificationsService);
    notificationsGateway = module.get(NotificationsGateway);
  });

  describe('onModuleInit', () => {
    it('should not start receiver when connection string is empty', async () => {
      await consumer.onModuleInit();

      expect(consumer.isConnected()).toBe(false);
    });
  });

  describe('onModuleDestroy', () => {
    it('should handle destroy gracefully when not connected', async () => {
      await expect(consumer.onModuleDestroy()).resolves.not.toThrow();
    });
  });

  describe('isConnected', () => {
    it('should return false initially', () => {
      expect(consumer.isConnected()).toBe(false);
    });
  });
});
