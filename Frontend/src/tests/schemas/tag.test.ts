import { describe, it, expect } from "vitest";
import { tagSchema } from "../../schemas/tag";

describe("tagSchema", () => {
  it("deve validar com apenas nome", () => {
    const result = tagSchema.safeParse({ name: "Alimentação" });
    expect(result.success).toBe(true);
  });

  it("deve validar com todos os campos", () => {
    const result = tagSchema.safeParse({
      name: "Alimentação",
      tagType: 0,
      description: "Gastos com comida",
      color: "#ff0000",
    });
    expect(result.success).toBe(true);
  });

  it("deve aceitar campos opcionais como null", () => {
    const result = tagSchema.safeParse({
      name: "Tag",
      tagType: null,
      description: null,
      color: null,
    });
    expect(result.success).toBe(true);
  });

  it("deve rejeitar nome vazio", () => {
    const result = tagSchema.safeParse({ name: "" });
    expect(result.success).toBe(false);
  });
});
