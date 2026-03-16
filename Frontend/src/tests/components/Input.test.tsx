import { describe, it, expect } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { Input } from "../../components/Input/index";

describe("Input", () => {
  it("deve renderizar input com label", () => {
    render(<Input label="Nome" id="name" />);
    expect(screen.getByLabelText("Nome")).toBeInTheDocument();
  });

  it("deve renderizar sem label", () => {
    render(<Input placeholder="Digite..." />);
    expect(screen.getByPlaceholderText("Digite...")).toBeInTheDocument();
  });

  it("deve exibir mensagem de erro", () => {
    render(<Input error="Campo obrigatório" />);
    expect(screen.getByText("Campo obrigatório")).toBeInTheDocument();
  });

  it("deve aplicar classe de erro no input", () => {
    render(<Input error="Erro" placeholder="test" />);
    const input = screen.getByPlaceholderText("test");
    expect(input.className).toContain("border-red-500");
  });

  it("deve alternar visibilidade da senha", async () => {
    render(<Input type="password" showPasswordToggle placeholder="senha" />);

    const input = screen.getByPlaceholderText("senha") as HTMLInputElement;
    expect(input.type).toBe("password");

    const toggleBtn = screen.getByRole("button", { name: "Mostrar senha" });
    await userEvent.click(toggleBtn);

    expect(input.type).toBe("text");

    const hideBtn = screen.getByRole("button", { name: "Esconder senha" });
    await userEvent.click(hideBtn);

    expect(input.type).toBe("password");
  });

  it("deve renderizar ícone", () => {
    render(<Input icon={<span data-testid="icon">🔍</span>} />);
    expect(screen.getByTestId("icon")).toBeInTheDocument();
  });

  it("deve aceitar digitação", async () => {
    render(<Input placeholder="Type here" />);
    const input = screen.getByPlaceholderText("Type here");

    await userEvent.type(input, "Hello");

    expect(input).toHaveValue("Hello");
  });
});
