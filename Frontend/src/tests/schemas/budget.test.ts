import { describe, it, expect } from "vitest";
import { budgetSchema } from "../../schemas/budget";

describe("budgetSchema", () => {
  it("deve validar dados válidos", () => {
    const result = budgetSchema.safeParse({ name: "Viagem", goal: 5000 });
    expect(result.success).toBe(true);
  });

  it("deve rejeitar nome vazio", () => {
    const result = budgetSchema.safeParse({ name: "", goal: 5000 });
    expect(result.success).toBe(false);
  });

  it("deve rejeitar goal zero", () => {
    const result = budgetSchema.safeParse({ name: "Test", goal: 0 });
    expect(result.success).toBe(false);
  });

  it("deve rejeitar goal negativo", () => {
    const result = budgetSchema.safeParse({ name: "Test", goal: -100 });
    expect(result.success).toBe(false);
  });
});
