import { describe, it, expect } from "vitest";
import { render, screen } from "@testing-library/react";
import { EmptyState } from "../../components/EmptyState/index";

describe("EmptyState", () => {
  it("deve renderizar com valores padrão", () => {
    render(<EmptyState />);
    expect(screen.getByText("Nenhum item encontrado")).toBeInTheDocument();
    expect(screen.getByText("Não há dados para exibir.")).toBeInTheDocument();
  });

  it("deve renderizar com título customizado", () => {
    render(<EmptyState title="Sem registros" />);
    expect(screen.getByText("Sem registros")).toBeInTheDocument();
  });

  it("deve renderizar com descrição customizada", () => {
    render(<EmptyState description="Crie um novo registro." />);
    expect(screen.getByText("Crie um novo registro.")).toBeInTheDocument();
  });

  it("deve renderizar ícone customizado", () => {
    render(<EmptyState icon={<span data-testid="custom-icon">📭</span>} />);
    expect(screen.getByTestId("custom-icon")).toBeInTheDocument();
  });
});
