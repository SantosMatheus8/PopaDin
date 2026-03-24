import { describe, it, expect } from "vitest";
import { goalSchema } from "../../schemas/goal";

describe("goalSchema", () => {
  it("deve validar dados válidos", () => {
    const result = goalSchema.safeParse({ name: "Viagem", targetAmount: 5000 });
    expect(result.success).toBe(true);
  });

  it("deve validar dados válidos com deadline", () => {
    const result = goalSchema.safeParse({ name: "Viagem", targetAmount: 5000, deadline: "2026-12-31" });
    expect(result.success).toBe(true);
  });

  it("deve rejeitar nome vazio", () => {
    const result = goalSchema.safeParse({ name: "", targetAmount: 5000 });
    expect(result.success).toBe(false);
  });

  it("deve rejeitar targetAmount zero", () => {
    const result = goalSchema.safeParse({ name: "Test", targetAmount: 0 });
    expect(result.success).toBe(false);
  });

  it("deve rejeitar targetAmount negativo", () => {
    const result = goalSchema.safeParse({ name: "Test", targetAmount: -100 });
    expect(result.success).toBe(false);
  });
});
