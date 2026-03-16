import { Test, TestingModule } from '@nestjs/testing';
import { NotFoundException } from '@nestjs/common';
import { NotificationsController } from './notifications.controller';
import { NotificationsService } from './notifications.service';

describe('NotificationsController', () => {
  let controller: NotificationsController;
  let service: jest.Mocked<NotificationsService>;

  const mockReq = { user: { userId: 1 } };

  beforeEach(async () => {
    const mockService = {
      findByUserId: jest.fn(),
      markAsRead: jest.fn(),
      markAllAsRead: jest.fn(),
      getUnreadCount: jest.fn(),
      deleteAll: jest.fn(),
    };

    const module: TestingModule = await Test.createTestingModule({
      controllers: [NotificationsController],
      providers: [{ provide: NotificationsService, useValue: mockService }],
    }).compile();

    controller = module.get<NotificationsController>(NotificationsController);
    service = module.get(NotificationsService);
  });

  describe('list', () => {
    it('should return paginated notifications', async () => {
      const expected = { data: [], total: 0, page: 1, limit: 10 };
      service.findByUserId.mockResolvedValue(expected);

      const result = await controller.list(mockReq, { page: 1, limit: 10 } as any);

      expect(service.findByUserId).toHaveBeenCalledWith(1, 1, 10);
      expect(result).toEqual(expected);
    });
  });

  describe('markAsRead', () => {
    it('should return updated notification', async () => {
      const notification = { _id: 'n1', read: true } as any;
      service.markAsRead.mockResolvedValue(notification);

      const result = await controller.markAsRead(mockReq, { id: 'n1' } as any);

      expect(service.markAsRead).toHaveBeenCalledWith('n1', 1);
      expect(result).toEqual(notification);
    });

    it('should throw NotFoundException when notification not found', async () => {
      service.markAsRead.mockResolvedValue(null);

      await expect(
        controller.markAsRead(mockReq, { id: 'invalid' } as any),
      ).rejects.toThrow(NotFoundException);
    });
  });

  describe('markAllAsRead', () => {
    it('should return modified count', async () => {
      service.markAllAsRead.mockResolvedValue(5);

      const result = await controller.markAllAsRead(mockReq);

      expect(service.markAllAsRead).toHaveBeenCalledWith(1);
      expect(result).toEqual({ modifiedCount: 5 });
    });
  });

  describe('unreadCount', () => {
    it('should return unread count', async () => {
      service.getUnreadCount.mockResolvedValue(3);

      const result = await controller.unreadCount(mockReq);

      expect(service.getUnreadCount).toHaveBeenCalledWith(1);
      expect(result).toEqual({ count: 3 });
    });
  });

  describe('deleteAll', () => {
    it('should return deleted count', async () => {
      service.deleteAll.mockResolvedValue(10);

      const result = await controller.deleteAll(mockReq);

      expect(service.deleteAll).toHaveBeenCalledWith(1);
      expect(result).toEqual({ deletedCount: 10 });
    });
  });
});
