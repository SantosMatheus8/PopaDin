import { CanActivate, ExecutionContext, Injectable, Logger } from '@nestjs/common';
import { ConfigService } from '@nestjs/config';
import * as jwt from 'jsonwebtoken';
import { Socket } from 'socket.io';

interface JwtPayload {
  sub: string;
  [key: string]: unknown;
}

@Injectable()
export class WsJwtGuard implements CanActivate {
  private readonly logger = new Logger(WsJwtGuard.name);

  constructor(private readonly configService: ConfigService) {}

  canActivate(context: ExecutionContext): boolean {
    const client = context.switchToWs().getClient<Socket>();
    const token =
      (client.handshake.auth?.token as string) ??
      (client.handshake.headers?.authorization?.replace('Bearer ', '') as string);

    if (!token) {
      this.logger.warn('WebSocket guard: token ausente');
      return false;
    }

    try {
      const secret = this.configService.get<string>('jwt.secret', '');
      const payload = jwt.verify(token, secret) as JwtPayload;
      const userId = parseInt(payload.sub, 10);

      if (isNaN(userId)) {
        this.logger.warn('WebSocket guard: userId inválido no token');
        return false;
      }

      client.data.userId = userId;
      return true;
    } catch (error) {
      this.logger.warn(
        `WebSocket guard: token inválido - ${error instanceof Error ? error.message : 'erro desconhecido'}`,
      );
      return false;
    }
  }
}
