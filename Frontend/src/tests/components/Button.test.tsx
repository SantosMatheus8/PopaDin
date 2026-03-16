import { describe, it, expect, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { Button } from "../../components/Button/index";

describe("Button", () => {
  it("deve renderizar com texto", () => {
    render(<Button>Click me</Button>);
    expect(screen.getByRole("button", { name: "Click me" })).toBeInTheDocument();
  });

  it("deve chamar onClick ao clicar", async () => {
    const onClick = vi.fn();
    render(<Button onClick={onClick}>Click</Button>);

    await userEvent.click(screen.getByRole("button"));

    expect(onClick).toHaveBeenCalledTimes(1);
  });

  it("deve ficar desabilitado quando disabled=true", () => {
    render(<Button disabled>Click</Button>);
    expect(screen.getByRole("button")).toBeDisabled();
  });

  it("deve renderizar ícone à esquerda por padrão", () => {
    render(<Button icon={<span data-testid="icon">★</span>}>Text</Button>);
    const button = screen.getByRole("button");
    const icon = screen.getByTestId("icon");
    expect(button).toContainElement(icon);
  });

  it("deve renderizar ícone à direita quando iconPosition=right", () => {
    render(
      <Button icon={<span data-testid="icon">★</span>} iconPosition="right">
        Text
      </Button>
    );
    expect(screen.getByTestId("icon")).toBeInTheDocument();
  });

  it("deve aplicar variante primary por padrão", () => {
    render(<Button>Click</Button>);
    const button = screen.getByRole("button");
    expect(button.className).toContain("bg-primary-500");
  });

  it("deve aplicar variante error", () => {
    render(<Button variant="error">Delete</Button>);
    const button = screen.getByRole("button");
    expect(button.className).toContain("bg-error");
  });

  it("deve aplicar tamanho sm", () => {
    render(<Button size="sm">Small</Button>);
    const button = screen.getByRole("button");
    expect(button.className).toContain("h-8");
  });

  it("deve aplicar tamanho lg", () => {
    render(<Button size="lg">Large</Button>);
    const button = screen.getByRole("button");
    expect(button.className).toContain("h-12");
  });
});
