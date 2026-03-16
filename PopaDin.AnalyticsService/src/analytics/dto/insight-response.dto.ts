import { IsOptional, IsString, IsNumber, Min } from 'class-validator';
import { Type } from 'class-transformer';

export class ListInsightsQueryDto {
  @IsOptional()
  @IsString()
  type?: string;

  @IsOptional()
  @Type(() => Number)
  @IsNumber()
  @Min(1)
  page?: number = 1;

  @IsOptional()
  @Type(() => Number)
  @IsNumber()
  @Min(1)
  limit?: number = 20;
}

export class InsightPeriodResponse {
  start!: string;
  end!: string;
}

export class InsightResponse {
  _id!: string;
  userId!: number;
  type!: string;
  title!: string;
  message!: string;
  severity!: string;
  data!: Record<string, unknown>;
  period!: InsightPeriodResponse;
  expiresAt!: string;
  createdAt!: string;
}

export class InsightListResponse {
  data!: InsightResponse[];
  total!: number;
  page!: number;
  limit!: number;
}
