import { describe, it, expect } from "vitest";
import { updateUserSchema } from "../../schemas/user";

describe("updateUserSchema", () => {
  it("deve validar dados completos", () => {
    const result = updateUserSchema.safeParse({
      name: "João",
      password: "123456",
    });
    expect(result.success).toBe(true);
  });

  it("deve validar sem password (opcional)", () => {
    const result = updateUserSchema.safeParse({
      name: "João",
    });
    expect(result.success).toBe(true);
  });

  it("deve aceitar password vazio (string vazia)", () => {
    const result = updateUserSchema.safeParse({
      name: "João",
      password: "",
    });
    expect(result.success).toBe(true);
  });

  it("deve rejeitar nome vazio", () => {
    const result = updateUserSchema.safeParse({
      name: "",
    });
    expect(result.success).toBe(false);
  });

  it("deve rejeitar password com menos de 6 caracteres", () => {
    const result = updateUserSchema.safeParse({
      name: "João",
      password: "12345",
    });
    expect(result.success).toBe(false);
  });
});
