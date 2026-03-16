import {
  Controller,
  Get,
  Post,
  Query,
  Request,
  UseGuards,
  HttpCode,
  HttpStatus,
} from '@nestjs/common';
import { JwtAuthGuard } from '../auth/jwt-auth.guard';
import { AnalyticsService } from './analytics.service';
import { ListInsightsQueryDto } from './dto/insight-response.dto';

interface AuthenticatedRequest {
  user: { userId: number };
}

@Controller('analytics')
@UseGuards(JwtAuthGuard)
export class AnalyticsController {
  constructor(private readonly analyticsService: AnalyticsService) {}

  @Get('insights')
  async listInsights(
    @Request() req: AuthenticatedRequest,
    @Query() query: ListInsightsQueryDto,
  ) {
    const { userId } = req.user;
    const { type, page = 1, limit = 20 } = query;

    const result = await this.analyticsService.getInsights(userId, type, page, limit);

    return {
      data: result.data,
      total: result.total,
      page,
      limit,
    };
  }

  @Get('insights/latest')
  async getLatestInsights(@Request() req: AuthenticatedRequest) {
    const { userId } = req.user;
    return this.analyticsService.getLatestInsights(userId);
  }

  @Get('insights/forecast')
  async getForecast(@Request() req: AuthenticatedRequest) {
    const { userId } = req.user;
    return this.analyticsService.getForecast(userId);
  }

  @Post('insights/refresh')
  @HttpCode(HttpStatus.OK)
  async refreshInsights(@Request() req: AuthenticatedRequest) {
    const { userId } = req.user;
    await this.analyticsService.processUserInsights(userId);
    return { message: 'Insights atualizados com sucesso' };
  }
}
