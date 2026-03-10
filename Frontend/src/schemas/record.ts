import { z } from "zod";

export const recordSchema = z.object({
  operation: z.number().min(0).max(1),
  value: z.number().min(0, "Valor deve ser maior ou igual a zero"),
  frequency: z.number().min(0).max(4),
  tagIds: z.array(z.number()),
});

export type RecordFormData = z.infer<typeof recordSchema>;

export const exportRecordsSchema = z.object({
  startDate: z.string().min(1, "Data inicial é obrigatória"),
  endDate: z.string().min(1, "Data final é obrigatória"),
});

export type ExportRecordsFormData = z.infer<typeof exportRecordsSchema>;
