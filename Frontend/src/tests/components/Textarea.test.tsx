import { describe, it, expect } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { Textarea } from "../../components/Textarea/index";

describe("Textarea", () => {
  it("deve renderizar com label", () => {
    render(<Textarea label="Descrição" id="desc" />);
    expect(screen.getByLabelText("Descrição")).toBeInTheDocument();
  });

  it("deve renderizar sem label", () => {
    render(<Textarea placeholder="Digite..." />);
    expect(screen.getByPlaceholderText("Digite...")).toBeInTheDocument();
  });

  it("deve exibir mensagem de erro", () => {
    render(<Textarea error="Campo obrigatório" />);
    expect(screen.getByText("Campo obrigatório")).toBeInTheDocument();
  });

  it("deve aceitar digitação", async () => {
    render(<Textarea placeholder="Write" />);
    const textarea = screen.getByPlaceholderText("Write");

    await userEvent.type(textarea, "Hello World");

    expect(textarea).toHaveValue("Hello World");
  });
});
