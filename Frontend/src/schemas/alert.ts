import { z } from "zod";

export const alertSchema = z.object({
  type: z.number().min(0).max(1),
  threshold: z.number().min(0.01, "O valor limite deve ser maior que zero"),
});

export type AlertFormData = z.infer<typeof alertSchema>;
