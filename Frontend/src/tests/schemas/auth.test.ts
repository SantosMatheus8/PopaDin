import { describe, it, expect } from "vitest";
import { loginSchema, registerSchema } from "../../schemas/auth";

describe("loginSchema", () => {
  it("deve validar dados válidos", () => {
    const result = loginSchema.safeParse({ email: "user@test.com", password: "123456" });
    expect(result.success).toBe(true);
  });

  it("deve rejeitar email inválido", () => {
    const result = loginSchema.safeParse({ email: "invalid", password: "123456" });
    expect(result.success).toBe(false);
  });

  it("deve rejeitar senha vazia", () => {
    const result = loginSchema.safeParse({ email: "user@test.com", password: "" });
    expect(result.success).toBe(false);
  });

  it("deve rejeitar campos ausentes", () => {
    const result = loginSchema.safeParse({});
    expect(result.success).toBe(false);
  });
});

describe("registerSchema", () => {
  it("deve validar dados completos", () => {
    const result = registerSchema.safeParse({
      name: "João",
      email: "joao@test.com",
      password: "123456",
      balance: 100,
    });
    expect(result.success).toBe(true);
  });

  it("deve validar sem balance (opcional)", () => {
    const result = registerSchema.safeParse({
      name: "João",
      email: "joao@test.com",
      password: "123456",
    });
    expect(result.success).toBe(true);
  });

  it("deve rejeitar nome vazio", () => {
    const result = registerSchema.safeParse({
      name: "",
      email: "joao@test.com",
      password: "123456",
    });
    expect(result.success).toBe(false);
  });

  it("deve rejeitar senha curta", () => {
    const result = registerSchema.safeParse({
      name: "João",
      email: "joao@test.com",
      password: "12345",
    });
    expect(result.success).toBe(false);
  });

  it("deve rejeitar balance negativo", () => {
    const result = registerSchema.safeParse({
      name: "João",
      email: "joao@test.com",
      password: "123456",
      balance: -1,
    });
    expect(result.success).toBe(false);
  });
});
