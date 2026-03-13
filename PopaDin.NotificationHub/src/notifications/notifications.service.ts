import { Injectable, Logger } from '@nestjs/common';
import { InjectModel } from '@nestjs/mongoose';
import { Model } from 'mongoose';
import { Notification } from './schemas/notification.schema';
import { NotificationEvent } from './interfaces/notification.interface';

@Injectable()
export class NotificationsService {
  private readonly logger = new Logger(NotificationsService.name);

  constructor(
    @InjectModel(Notification.name)
    private readonly notificationModel: Model<Notification>,
  ) {}

  async create(event: NotificationEvent): Promise<Notification> {
    this.logger.log(
      `Criando notificação do tipo ${event.type} para o usuário ${event.userId}`,
    );

    const notification = new this.notificationModel({
      userId: event.userId,
      type: event.type,
      title: event.title,
      message: event.message,
      metadata: event.metadata ?? {},
      read: false,
      createdAt: new Date(),
      readAt: null,
    });

    return notification.save();
  }

  async findByUserId(
    userId: number,
    page: number,
    limit: number,
  ): Promise<{ data: Notification[]; total: number; page: number; limit: number }> {
    const skip = (page - 1) * limit;

    const [data, total] = await Promise.all([
      this.notificationModel
        .find({ userId })
        .sort({ createdAt: -1 })
        .skip(skip)
        .limit(limit)
        .exec(),
      this.notificationModel.countDocuments({ userId }),
    ]);

    return { data, total, page, limit };
  }

  async markAsRead(id: string, userId: number): Promise<Notification | null> {
    return this.notificationModel.findOneAndUpdate(
      { _id: id, userId },
      { read: true, readAt: new Date() },
      { new: true },
    );
  }

  async markAllAsRead(userId: number): Promise<number> {
    const result = await this.notificationModel.updateMany(
      { userId, read: false },
      { read: true, readAt: new Date() },
    );
    return result.modifiedCount;
  }

  async getUnreadCount(userId: number): Promise<number> {
    return this.notificationModel.countDocuments({ userId, read: false });
  }

  async deleteAll(userId: number): Promise<number> {
    this.logger.log(`Removendo todas as notificações do usuário ${userId}`);
    const result = await this.notificationModel.deleteMany({ userId });
    return result.deletedCount;
  }
}
