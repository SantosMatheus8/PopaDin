import { z } from "zod";

export const recordSchema = z.object({
  name: z.string().min(1, "Nome é obrigatório"),
  operation: z.number().min(0).max(1),
  value: z.number().gt(0, "Valor deve ser maior que zero"),
  frequency: z.number().min(0).max(5),
  tagIds: z.array(z.number()),
  referenceDate: z.string().optional(),
});

export type RecordFormData = z.infer<typeof recordSchema>;

export const exportRecordsSchema = z.object({
  startDate: z.string().min(1, "Data inicial é obrigatória"),
  endDate: z.string().min(1, "Data final é obrigatória"),
});

export type ExportRecordsFormData = z.infer<typeof exportRecordsSchema>;
