import { NotificationsGateway } from './notifications.gateway';
import { NotificationsService } from './notifications.service';
import { ConfigService } from '@nestjs/config';
import * as jwt from 'jsonwebtoken';

jest.mock('jsonwebtoken');

describe('NotificationsGateway', () => {
  let gateway: NotificationsGateway;
  let notificationsService: jest.Mocked<NotificationsService>;
  let configService: jest.Mocked<ConfigService>;

  const mockServer = {
    to: jest.fn().mockReturnThis(),
    emit: jest.fn(),
  };

  beforeEach(() => {
    notificationsService = {
      getUnreadCount: jest.fn().mockResolvedValue(5),
    } as any;

    configService = {
      get: jest.fn().mockReturnValue('test-secret'),
    } as any;

    gateway = new NotificationsGateway(configService, notificationsService);
    gateway.server = mockServer as any;
  });

  afterEach(() => {
    jest.clearAllMocks();
  });

  describe('handleConnection', () => {
    it('should authenticate and register socket with valid token', async () => {
      (jwt.verify as jest.Mock).mockReturnValue({ sub: '1' });

      const client = {
        id: 'socket-1',
        handshake: { auth: { token: 'valid-token' }, headers: {} },
        data: {},
        emit: jest.fn(),
        disconnect: jest.fn(),
      } as any;

      await gateway.handleConnection(client);

      expect(client.data.userId).toBe(1);
      expect(client.emit).toHaveBeenCalledWith('notification:count', { count: 5 });
      expect(client.disconnect).not.toHaveBeenCalled();
    });

    it('should disconnect client without token', async () => {
      const client = {
        id: 'socket-2',
        handshake: { auth: {}, headers: {} },
        data: {},
        disconnect: jest.fn(),
      } as any;

      await gateway.handleConnection(client);

      expect(client.disconnect).toHaveBeenCalled();
    });

    it('should disconnect client with invalid token', async () => {
      (jwt.verify as jest.Mock).mockImplementation(() => {
        throw new Error('invalid token');
      });

      const client = {
        id: 'socket-3',
        handshake: { auth: { token: 'bad-token' }, headers: {} },
        data: {},
        disconnect: jest.fn(),
      } as any;

      await gateway.handleConnection(client);

      expect(client.disconnect).toHaveBeenCalled();
    });

    it('should disconnect client with NaN userId', async () => {
      (jwt.verify as jest.Mock).mockReturnValue({ sub: 'not-a-number' });

      const client = {
        id: 'socket-4',
        handshake: { auth: { token: 'token' }, headers: {} },
        data: {},
        disconnect: jest.fn(),
      } as any;

      await gateway.handleConnection(client);

      expect(client.disconnect).toHaveBeenCalled();
    });

    it('should extract token from authorization header', async () => {
      (jwt.verify as jest.Mock).mockReturnValue({ sub: '2' });

      const client = {
        id: 'socket-5',
        handshake: {
          auth: {},
          headers: { authorization: 'Bearer header-token' },
        },
        data: {},
        emit: jest.fn(),
        disconnect: jest.fn(),
      } as any;

      await gateway.handleConnection(client);

      expect(jwt.verify).toHaveBeenCalledWith('header-token', 'test-secret');
      expect(client.data.userId).toBe(2);
    });
  });

  describe('handleDisconnect', () => {
    it('should remove socket from userSockets', async () => {
      (jwt.verify as jest.Mock).mockReturnValue({ sub: '1' });

      const client = {
        id: 'socket-1',
        handshake: { auth: { token: 'token' }, headers: {} },
        data: {},
        emit: jest.fn(),
        disconnect: jest.fn(),
      } as any;

      await gateway.handleConnection(client);
      expect(gateway.isUserConnected(1)).toBe(true);

      gateway.handleDisconnect(client);
      expect(gateway.isUserConnected(1)).toBe(false);
    });

    it('should handle disconnect for unauthenticated client', () => {
      const client = { id: 'socket-x', data: {} } as any;

      expect(() => gateway.handleDisconnect(client)).not.toThrow();
    });
  });

  describe('sendNotification', () => {
    it('should emit to connected user sockets', async () => {
      (jwt.verify as jest.Mock).mockReturnValue({ sub: '1' });

      const client = {
        id: 'socket-1',
        handshake: { auth: { token: 'token' }, headers: {} },
        data: {},
        emit: jest.fn(),
        disconnect: jest.fn(),
      } as any;

      await gateway.handleConnection(client);

      const notification = { _id: 'n1', title: 'Test' } as any;
      await gateway.sendNotification(1, notification);

      expect(mockServer.to).toHaveBeenCalledWith('socket-1');
      expect(mockServer.emit).toHaveBeenCalledWith('notification:new', notification);
      expect(mockServer.emit).toHaveBeenCalledWith('notification:count', { count: 5 });
    });

    it('should not emit when user is not connected', async () => {
      const notification = { _id: 'n1' } as any;
      await gateway.sendNotification(999, notification);

      expect(mockServer.to).not.toHaveBeenCalled();
    });
  });

  describe('isUserConnected', () => {
    it('should return false for unknown user', () => {
      expect(gateway.isUserConnected(999)).toBe(false);
    });
  });
});
