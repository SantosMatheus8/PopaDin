import {
  Controller,
  Delete,
  Get,
  Patch,
  Param,
  Query,
  UseGuards,
  Request,
  NotFoundException,
} from '@nestjs/common';
import { AuthGuard } from '@nestjs/passport';
import { NotificationsService } from './notifications.service';
import { ListNotificationsDto } from './dto/notification.dto';
import { MarkReadParamDto } from './dto/mark-read.dto';

interface AuthenticatedRequest {
  user: { userId: number };
}

@Controller('notifications')
@UseGuards(AuthGuard('jwt'))
export class NotificationsController {
  constructor(private readonly notificationsService: NotificationsService) {}

  @Get()
  async list(
    @Request() req: AuthenticatedRequest,
    @Query() query: ListNotificationsDto,
  ) {
    return this.notificationsService.findByUserId(
      req.user.userId,
      query.page,
      query.limit,
    );
  }

  @Patch(':id/read')
  async markAsRead(
    @Request() req: AuthenticatedRequest,
    @Param() params: MarkReadParamDto,
  ) {
    const notification = await this.notificationsService.markAsRead(
      params.id,
      req.user.userId,
    );

    if (!notification) {
      throw new NotFoundException('Notificação não encontrada');
    }

    return notification;
  }

  @Patch('read-all')
  async markAllAsRead(@Request() req: AuthenticatedRequest) {
    const count = await this.notificationsService.markAllAsRead(req.user.userId);
    return { modifiedCount: count };
  }

  @Get('unread-count')
  async unreadCount(@Request() req: AuthenticatedRequest) {
    const count = await this.notificationsService.getUnreadCount(req.user.userId);
    return { count };
  }

  @Delete()
  async deleteAll(@Request() req: AuthenticatedRequest) {
    const count = await this.notificationsService.deleteAll(req.user.userId);
    return { deletedCount: count };
  }
}
