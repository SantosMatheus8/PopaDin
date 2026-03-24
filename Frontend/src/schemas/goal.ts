import { z } from "zod";

export const goalSchema = z.object({
  name: z.string().min(1, "Nome é obrigatório"),
  targetAmount: z.number().min(1, "O valor da meta deve ser maior que um"),
  deadline: z.string().nullable().optional(),
});

export type GoalFormData = z.infer<typeof goalSchema>;
