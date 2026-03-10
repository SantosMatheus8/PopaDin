import { z } from "zod";
import { AlertType } from "../types/enums";

export const alertSchema = z.object({
  type: z.nativeEnum(AlertType),
  threshold: z.number().min(0.01, "O valor limite deve ser maior que zero"),
});

export type AlertFormData = z.infer<typeof alertSchema>;
