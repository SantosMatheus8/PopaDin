import { describe, it, expect } from "vitest";
import { render, screen } from "@testing-library/react";
import { Badge } from "../../components/Badge/index";

describe("Badge", () => {
  it("deve renderizar com texto", () => {
    render(<Badge>Ativo</Badge>);
    expect(screen.getByText("Ativo")).toBeInTheDocument();
  });

  it("deve aplicar variante default por padrão", () => {
    render(<Badge>Tag</Badge>);
    expect(screen.getByText("Tag").className).toContain("bg-gray-100");
  });

  it("deve aplicar variante success", () => {
    render(<Badge variant="success">Sucesso</Badge>);
    expect(screen.getByText("Sucesso").className).toContain("bg-green-100");
  });

  it("deve aplicar variante warning", () => {
    render(<Badge variant="warning">Atenção</Badge>);
    expect(screen.getByText("Atenção").className).toContain("bg-amber-100");
  });

  it("deve aplicar variante error", () => {
    render(<Badge variant="error">Erro</Badge>);
    expect(screen.getByText("Erro").className).toContain("bg-red-100");
  });

  it("deve aplicar variante info", () => {
    render(<Badge variant="info">Info</Badge>);
    expect(screen.getByText("Info").className).toContain("bg-blue-100");
  });

  it("deve aceitar className customizado", () => {
    render(<Badge className="ml-2">Tag</Badge>);
    expect(screen.getByText("Tag").className).toContain("ml-2");
  });

  it("deve aceitar style inline", () => {
    render(<Badge style={{ color: "rgb(255, 0, 0)" }}>Tag</Badge>);
    expect(screen.getByText("Tag").style.color).toBe("rgb(255, 0, 0)");
  });
});
