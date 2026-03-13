import {
  WebSocketGateway,
  WebSocketServer,
  OnGatewayConnection,
  OnGatewayDisconnect,
} from '@nestjs/websockets';
import { Logger } from '@nestjs/common';
import { Server, Socket } from 'socket.io';
import { ConfigService } from '@nestjs/config';
import * as jwt from 'jsonwebtoken';
import { NotificationsService } from './notifications.service';
import { Notification } from './schemas/notification.schema';

interface JwtPayload {
  sub: string;
  [key: string]: unknown;
}

@WebSocketGateway({
  namespace: '/notifications',
  cors: { origin: true, credentials: true },
})
export class NotificationsGateway
  implements OnGatewayConnection, OnGatewayDisconnect
{
  @WebSocketServer()
  server!: Server;

  private readonly logger = new Logger(NotificationsGateway.name);
  private readonly userSockets = new Map<number, Set<string>>();

  constructor(
    private readonly configService: ConfigService,
    private readonly notificationsService: NotificationsService,
  ) {}

  async handleConnection(client: Socket): Promise<void> {
    try {
      const token =
        (client.handshake.auth?.token as string) ??
        (client.handshake.headers?.authorization?.replace('Bearer ', '') as string);

      if (!token) {
        this.logger.warn('Conexão WebSocket rejeitada: token ausente');
        client.disconnect();
        return;
      }

      const secret = this.configService.get<string>('jwt.secret', '');
      const payload = jwt.verify(token, secret) as JwtPayload;
      const userId = parseInt(payload.sub, 10);

      if (isNaN(userId)) {
        this.logger.warn('Conexão WebSocket rejeitada: userId inválido');
        client.disconnect();
        return;
      }

      client.data.userId = userId;

      if (!this.userSockets.has(userId)) {
        this.userSockets.set(userId, new Set());
      }
      this.userSockets.get(userId)!.add(client.id);

      this.logger.log(`Usuário ${userId} conectado via WebSocket (${client.id})`);

      const unreadCount = await this.notificationsService.getUnreadCount(userId);
      client.emit('notification:count', { count: unreadCount });
    } catch (error) {
      this.logger.warn(
        `Conexão WebSocket rejeitada: token inválido - ${error instanceof Error ? error.message : 'erro desconhecido'}`,
      );
      client.disconnect();
    }
  }

  handleDisconnect(client: Socket): void {
    const userId = client.data.userId as number | undefined;
    if (userId !== undefined) {
      const sockets = this.userSockets.get(userId);
      if (sockets) {
        sockets.delete(client.id);
        if (sockets.size === 0) {
          this.userSockets.delete(userId);
        }
      }
      this.logger.log(`Usuário ${userId} desconectado (${client.id})`);
    }
  }

  async sendNotification(
    userId: number,
    notification: Notification,
  ): Promise<void> {
    const sockets = this.userSockets.get(userId);
    if (!sockets || sockets.size === 0) {
      this.logger.log(
        `Usuário ${userId} não está conectado, notificação salva apenas no banco`,
      );
      return;
    }

    const unreadCount = await this.notificationsService.getUnreadCount(userId);

    for (const socketId of sockets) {
      this.server.to(socketId).emit('notification:new', notification);
      this.server.to(socketId).emit('notification:count', { count: unreadCount });
    }

    this.logger.log(
      `Notificação enviada via WebSocket para o usuário ${userId} (${sockets.size} conexões)`,
    );
  }

  isUserConnected(userId: number): boolean {
    const sockets = this.userSockets.get(userId);
    return sockets !== undefined && sockets.size > 0;
  }
}
