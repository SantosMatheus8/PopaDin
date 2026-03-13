import { IsMongoId } from 'class-validator';

export class MarkReadParamDto {
  @IsMongoId()
  id!: string;
}
