import { describe, it, expect } from "vitest";
import { recordSchema, exportRecordsSchema } from "../../schemas/record";

describe("recordSchema", () => {
  it("deve validar registro completo", () => {
    const result = recordSchema.safeParse({
      name: "Salário",
      operation: 1,
      value: 5000,
      frequency: 0,
      tagIds: [1, 2],
    });
    expect(result.success).toBe(true);
  });

  it("deve validar com campos opcionais", () => {
    const result = recordSchema.safeParse({
      name: "Compra",
      operation: 0,
      value: 100,
      frequency: 5,
      tagIds: [],
      referenceDate: "2024-03-15",
      installments: 3,
      recurrenceEndDate: "2025-03-15",
    });
    expect(result.success).toBe(true);
  });

  it("deve rejeitar nome vazio", () => {
    const result = recordSchema.safeParse({
      name: "",
      operation: 0,
      value: 100,
      frequency: 0,
      tagIds: [],
    });
    expect(result.success).toBe(false);
  });

  it("deve rejeitar valor zero", () => {
    const result = recordSchema.safeParse({
      name: "Test",
      operation: 0,
      value: 0,
      frequency: 0,
      tagIds: [],
    });
    expect(result.success).toBe(false);
  });

  it("deve rejeitar valor negativo", () => {
    const result = recordSchema.safeParse({
      name: "Test",
      operation: 0,
      value: -10,
      frequency: 0,
      tagIds: [],
    });
    expect(result.success).toBe(false);
  });

  it("deve rejeitar installments menor que 2", () => {
    const result = recordSchema.safeParse({
      name: "Test",
      operation: 0,
      value: 100,
      frequency: 0,
      tagIds: [],
      installments: 1,
    });
    expect(result.success).toBe(false);
  });

  it("deve rejeitar installments maior que 48", () => {
    const result = recordSchema.safeParse({
      name: "Test",
      operation: 0,
      value: 100,
      frequency: 0,
      tagIds: [],
      installments: 49,
    });
    expect(result.success).toBe(false);
  });

  it("deve rejeitar operation inválida", () => {
    const result = recordSchema.safeParse({
      name: "Test",
      operation: 2,
      value: 100,
      frequency: 0,
      tagIds: [],
    });
    expect(result.success).toBe(false);
  });

  it("deve rejeitar frequency inválida", () => {
    const result = recordSchema.safeParse({
      name: "Test",
      operation: 0,
      value: 100,
      frequency: 6,
      tagIds: [],
    });
    expect(result.success).toBe(false);
  });
});

describe("exportRecordsSchema", () => {
  it("deve validar datas válidas", () => {
    const result = exportRecordsSchema.safeParse({
      startDate: "2024-01-01",
      endDate: "2024-12-31",
    });
    expect(result.success).toBe(true);
  });

  it("deve rejeitar data inicial vazia", () => {
    const result = exportRecordsSchema.safeParse({
      startDate: "",
      endDate: "2024-12-31",
    });
    expect(result.success).toBe(false);
  });

  it("deve rejeitar data final vazia", () => {
    const result = exportRecordsSchema.safeParse({
      startDate: "2024-01-01",
      endDate: "",
    });
    expect(result.success).toBe(false);
  });
});
