import { Test, TestingModule } from '@nestjs/testing';
import { getModelToken } from '@nestjs/mongoose';
import { NotificationsService } from './notifications.service';
import { Notification } from './schemas/notification.schema';
import { NotificationEvent } from './interfaces/notification.interface';

describe('NotificationsService', () => {
  let service: NotificationsService;
  let mockModel: any;

  const mockNotification = {
    _id: 'notif-1',
    userId: 1,
    type: 'BALANCE_BELOW',
    title: 'Test',
    message: 'Test message',
    metadata: {},
    read: false,
    createdAt: new Date(),
    readAt: null,
    save: jest.fn(),
  };

  beforeEach(async () => {
    const saveFn = jest.fn().mockResolvedValue(mockNotification);

    mockModel = jest.fn().mockImplementation(() => ({
      ...mockNotification,
      save: saveFn,
    }));

    mockModel.find = jest.fn().mockReturnValue({
      sort: jest.fn().mockReturnValue({
        skip: jest.fn().mockReturnValue({
          limit: jest.fn().mockReturnValue({
            exec: jest.fn().mockResolvedValue([mockNotification]),
          }),
        }),
      }),
    });

    mockModel.countDocuments = jest.fn().mockResolvedValue(1);

    mockModel.findOneAndUpdate = jest.fn().mockResolvedValue(mockNotification);

    mockModel.updateMany = jest.fn().mockResolvedValue({ modifiedCount: 3 });

    mockModel.deleteMany = jest.fn().mockResolvedValue({ deletedCount: 5 });

    const module: TestingModule = await Test.createTestingModule({
      providers: [
        NotificationsService,
        { provide: getModelToken(Notification.name), useValue: mockModel },
      ],
    }).compile();

    service = module.get<NotificationsService>(NotificationsService);
  });

  describe('create', () => {
    it('should create a notification from event', async () => {
      const event: NotificationEvent = {
        userId: 1,
        type: 'BALANCE_BELOW',
        title: 'Alerta',
        message: 'Saldo baixo',
        metadata: { threshold: 500 },
      };

      const result = await service.create(event);

      expect(mockModel).toHaveBeenCalledWith(
        expect.objectContaining({
          userId: 1,
          type: 'BALANCE_BELOW',
          title: 'Alerta',
          message: 'Saldo baixo',
          read: false,
        }),
      );
      expect(result).toBeDefined();
    });

    it('should default metadata to empty object when not provided', async () => {
      const event: NotificationEvent = {
        userId: 1,
        type: 'EXPORT_COMPLETED',
        title: 'Export',
        message: 'Done',
      };

      await service.create(event);

      expect(mockModel).toHaveBeenCalledWith(
        expect.objectContaining({ metadata: {} }),
      );
    });
  });

  describe('findByUserId', () => {
    it('should return paginated results', async () => {
      const result = await service.findByUserId(1, 1, 10);

      expect(result).toEqual({
        data: [mockNotification],
        total: 1,
        page: 1,
        limit: 10,
      });
    });

    it('should calculate skip correctly for page 2', async () => {
      await service.findByUserId(1, 2, 10);

      expect(mockModel.find).toHaveBeenCalledWith({ userId: 1 });
    });
  });

  describe('markAsRead', () => {
    it('should update notification as read', async () => {
      const result = await service.markAsRead('notif-1', 1);

      expect(mockModel.findOneAndUpdate).toHaveBeenCalledWith(
        { _id: 'notif-1', userId: 1 },
        expect.objectContaining({ read: true }),
        { new: true },
      );
      expect(result).toBeDefined();
    });

    it('should return null when notification not found', async () => {
      mockModel.findOneAndUpdate.mockResolvedValue(null);

      const result = await service.markAsRead('invalid', 1);

      expect(result).toBeNull();
    });
  });

  describe('markAllAsRead', () => {
    it('should update all unread notifications', async () => {
      const result = await service.markAllAsRead(1);

      expect(mockModel.updateMany).toHaveBeenCalledWith(
        { userId: 1, read: false },
        expect.objectContaining({ read: true }),
      );
      expect(result).toBe(3);
    });
  });

  describe('getUnreadCount', () => {
    it('should return count of unread notifications', async () => {
      mockModel.countDocuments.mockResolvedValue(5);

      const result = await service.getUnreadCount(1);

      expect(mockModel.countDocuments).toHaveBeenCalledWith({ userId: 1, read: false });
      expect(result).toBe(5);
    });
  });

  describe('deleteAll', () => {
    it('should delete all notifications for user', async () => {
      const result = await service.deleteAll(1);

      expect(mockModel.deleteMany).toHaveBeenCalledWith({ userId: 1 });
      expect(result).toBe(5);
    });
  });
});
