import { describe, it, expect } from "vitest";
import { formatCurrency, formatDate, formatDateTime, formatFileSize } from "../../lib/format";

describe("formatCurrency", () => {
  it("deve formatar valor positivo em BRL", () => {
    const result = formatCurrency(1234.56);
    expect(result).toContain("1.234,56");
  });

  it("deve formatar zero", () => {
    const result = formatCurrency(0);
    expect(result).toContain("0,00");
  });

  it("deve formatar valor negativo", () => {
    const result = formatCurrency(-500);
    expect(result).toContain("500,00");
  });

  it("deve formatar valores grandes", () => {
    const result = formatCurrency(1000000);
    expect(result).toContain("1.000.000,00");
  });
});

describe("formatDate", () => {
  it("deve formatar data ISO para formato brasileiro", () => {
    const result = formatDate("2024-03-15T00:00:00Z");
    expect(result).toBe("15/03/2024");
  });

  it("deve retornar traço para null", () => {
    expect(formatDate(null)).toBe("-");
  });

  it("deve retornar traço para string vazia", () => {
    expect(formatDate("")).toBe("-");
  });
});

describe("formatDateTime", () => {
  it("deve formatar data e hora", () => {
    const result = formatDateTime("2024-03-15T14:30:00Z");
    expect(result).toMatch(/15\/03\/2024/);
  });

  it("deve retornar traço para null", () => {
    expect(formatDateTime(null)).toBe("-");
  });

  it("deve retornar traço para string vazia", () => {
    expect(formatDateTime("")).toBe("-");
  });
});

describe("formatFileSize", () => {
  it("deve formatar bytes", () => {
    expect(formatFileSize(500)).toBe("500 B");
  });

  it("deve formatar kilobytes", () => {
    expect(formatFileSize(1536)).toBe("1.5 KB");
  });

  it("deve formatar megabytes", () => {
    expect(formatFileSize(2621440)).toBe("2.5 MB");
  });

  it("deve formatar exatamente 1 KB", () => {
    expect(formatFileSize(1024)).toBe("1.0 KB");
  });

  it("deve formatar exatamente 1 MB", () => {
    expect(formatFileSize(1048576)).toBe("1.0 MB");
  });

  it("deve formatar 0 bytes", () => {
    expect(formatFileSize(0)).toBe("0 B");
  });
});
