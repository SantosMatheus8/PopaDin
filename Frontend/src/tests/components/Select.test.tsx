import { describe, it, expect } from "vitest";
import { render, screen } from "@testing-library/react";
import { Select } from "../../components/Select/index";

const options = [
  { value: "1", label: "Opção 1" },
  { value: "2", label: "Opção 2" },
  { value: "3", label: "Opção 3" },
];

describe("Select", () => {
  it("deve renderizar com label", () => {
    render(<Select label="Tipo" id="type" options={options} />);
    expect(screen.getByLabelText("Tipo")).toBeInTheDocument();
  });

  it("deve renderizar todas as opções", () => {
    render(<Select options={options} />);
    expect(screen.getAllByRole("option")).toHaveLength(3);
  });

  it("deve renderizar placeholder como opção desabilitada", () => {
    render(<Select options={options} placeholder="Selecione..." />);
    const placeholder = screen.getByText("Selecione...");
    expect(placeholder).toBeInTheDocument();
    expect(placeholder).toBeDisabled();
  });

  it("deve exibir mensagem de erro", () => {
    render(<Select options={options} error="Selecione uma opção" />);
    expect(screen.getByText("Selecione uma opção")).toBeInTheDocument();
  });

  it("deve aplicar classe de erro", () => {
    render(<Select options={options} error="Erro" />);
    const select = screen.getByRole("combobox");
    expect(select.className).toContain("border-red-500");
  });
});
