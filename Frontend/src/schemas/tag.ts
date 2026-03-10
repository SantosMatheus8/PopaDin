import { z } from "zod";

export const tagSchema = z.object({
  name: z.string().min(1, "Nome é obrigatório"),
  tagType: z.number().nullable().optional(),
  description: z.string().nullable().optional(),
});

export type TagFormData = z.infer<typeof tagSchema>;
