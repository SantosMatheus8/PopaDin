import {
  Injectable,
  Logger,
  OnModuleInit,
  OnModuleDestroy,
} from '@nestjs/common';
import { ConfigService } from '@nestjs/config';
import {
  ServiceBusClient,
  ServiceBusReceiver,
  ServiceBusReceivedMessage,
  ProcessErrorArgs,
} from '@azure/service-bus';
import { NotificationsService } from '../notifications/notifications.service';
import { NotificationsGateway } from '../notifications/notifications.gateway';
import { NotificationEvent } from '../notifications/interfaces/notification.interface';

@Injectable()
export class ServiceBusConsumer implements OnModuleInit, OnModuleDestroy {
  private readonly logger = new Logger(ServiceBusConsumer.name);
  private client: ServiceBusClient | null = null;
  private receiver: ServiceBusReceiver | null = null;
  private isRunning = false;

  constructor(
    private readonly configService: ConfigService,
    private readonly notificationsService: NotificationsService,
    private readonly notificationsGateway: NotificationsGateway,
  ) {}

  async onModuleInit(): Promise<void> {
    const connectionString = this.configService.get<string>(
      'serviceBus.connectionString',
      '',
    );
    const queueName = this.configService.get<string>(
      'serviceBus.queueName',
      'notifications',
    );

    if (!connectionString) {
      this.logger.warn(
        'Service Bus connection string não configurada, consumidor não será iniciado',
      );
      return;
    }

    const cleanedConnectionString = connectionString
      .split(';')
      .filter(
        (part) =>
          !part.trim().toLowerCase().startsWith('entitypath='),
      )
      .join(';');

    this.client = new ServiceBusClient(cleanedConnectionString);
    this.receiver = this.client.createReceiver(queueName, {
      receiveMode: 'peekLock',
    });

    this.isRunning = true;
    this.startReceiving();

    this.logger.log(
      `Consumidor do Service Bus iniciado na fila "${queueName}"`,
    );
  }

  async onModuleDestroy(): Promise<void> {
    this.isRunning = false;

    if (this.receiver) {
      await this.receiver.close();
    }
    if (this.client) {
      await this.client.close();
    }

    this.logger.log('Consumidor do Service Bus encerrado');
  }

  private startReceiving(): void {
    if (!this.receiver) return;

    const processMessage = async (
      message: ServiceBusReceivedMessage,
    ): Promise<void> => {
      try {
        const body = message.body as Record<string, unknown>;
        this.logger.log(`Mensagem recebida: ${JSON.stringify(body)}`);

        const event: NotificationEvent = {
          userId: body.userId as number,
          type: body.type as NotificationEvent['type'],
          title: body.title as string,
          message: body.message as string,
          metadata: (body.metadata as Record<string, unknown>) ?? {},
        };

        const notification = await this.notificationsService.create(event);
        await this.notificationsGateway.sendNotification(
          event.userId,
          notification,
        );

        await this.receiver!.completeMessage(message);

        this.logger.log(
          `Notificação ${notification._id} processada para o usuário ${event.userId}`,
        );
      } catch (error) {
        this.logger.error(
          `Erro ao processar mensagem: ${error instanceof Error ? error.message : 'erro desconhecido'}`,
        );

        try {
          await this.receiver!.abandonMessage(message);
        } catch (abandonError) {
          this.logger.error(
            `Erro ao abandonar mensagem: ${abandonError instanceof Error ? abandonError.message : 'erro desconhecido'}`,
          );
        }
      }
    };

    const processError = async (args: ProcessErrorArgs): Promise<void> => {
      this.logger.error(`Erro no Service Bus: ${args.error.message}`);
    };

    this.receiver.subscribe(
      {
        processMessage,
        processError,
      },
      {
        maxConcurrentCalls: 1,
        autoCompleteMessages: false,
      },
    );
  }

  isConnected(): boolean {
    return this.isRunning && this.receiver !== null;
  }
}
