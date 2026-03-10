import { z } from "zod";

export const updateUserSchema = z.object({
  name: z.string().min(1, "Nome é obrigatório"),
  password: z.string().min(6, "Senha deve ter no mínimo 6 caracteres").optional().or(z.literal("")),
  balance: z.number().min(0, "Saldo deve ser maior ou igual a zero"),
});

export type UpdateUserFormData = z.infer<typeof updateUserSchema>;
