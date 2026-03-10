import { z } from "zod";
import { AlertType } from "../types/enums";

export const alertSchema = z.object({
  type: z.nativeEnum(AlertType),
  threshold: z.number().min(0.01, "O threshold deve ser maior que zero"),
  channel: z.string().min(1, "Canal é obrigatório"),
});

export type AlertFormData = z.infer<typeof alertSchema>;
