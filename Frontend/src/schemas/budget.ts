import { z } from "zod";

export const budgetSchema = z.object({
  name: z.string().min(1, "Nome é obrigatório"),
  goal: z.number().min(1, "A meta deve ser maior que um"),
});

export type BudgetFormData = z.infer<typeof budgetSchema>;
