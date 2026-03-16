import { describe, it, expect } from "vitest";
import { alertSchema } from "../../schemas/alert";

describe("alertSchema", () => {
  it("deve validar alerta BALANCE_BELOW", () => {
    const result = alertSchema.safeParse({ type: 0, threshold: 500 });
    expect(result.success).toBe(true);
  });

  it("deve validar alerta BUDGET_ABOVE", () => {
    const result = alertSchema.safeParse({ type: 1, threshold: 1000 });
    expect(result.success).toBe(true);
  });

  it("deve rejeitar type inválido", () => {
    const result = alertSchema.safeParse({ type: 2, threshold: 500 });
    expect(result.success).toBe(false);
  });

  it("deve rejeitar threshold zero", () => {
    const result = alertSchema.safeParse({ type: 0, threshold: 0 });
    expect(result.success).toBe(false);
  });

  it("deve rejeitar threshold negativo", () => {
    const result = alertSchema.safeParse({ type: 0, threshold: -10 });
    expect(result.success).toBe(false);
  });

  it("deve aceitar threshold mínimo 0.01", () => {
    const result = alertSchema.safeParse({ type: 0, threshold: 0.01 });
    expect(result.success).toBe(true);
  });
});
