import { describe, it, expect, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { Pagination } from "../../components/Pagination/index";

describe("Pagination", () => {
  it("não deve renderizar quando totalPages <= 1", () => {
    const { container } = render(
      <Pagination page={1} totalPages={1} onPageChange={vi.fn()} />
    );
    expect(container.innerHTML).toBe("");
  });

  it("deve renderizar páginas quando totalPages > 1", () => {
    render(<Pagination page={1} totalPages={3} onPageChange={vi.fn()} />);
    expect(screen.getByText("1")).toBeInTheDocument();
    expect(screen.getByText("2")).toBeInTheDocument();
    expect(screen.getByText("3")).toBeInTheDocument();
  });

  it("deve chamar onPageChange ao clicar em uma página", async () => {
    const onPageChange = vi.fn();
    render(<Pagination page={1} totalPages={3} onPageChange={onPageChange} />);

    await userEvent.click(screen.getByText("2"));

    expect(onPageChange).toHaveBeenCalledWith(2);
  });

  it("deve desabilitar botão anterior na primeira página", () => {
    render(<Pagination page={1} totalPages={3} onPageChange={vi.fn()} />);
    const buttons = screen.getAllByRole("button");
    expect(buttons[0]).toBeDisabled();
  });

  it("deve desabilitar botão próximo na última página", () => {
    render(<Pagination page={3} totalPages={3} onPageChange={vi.fn()} />);
    const buttons = screen.getAllByRole("button");
    expect(buttons[buttons.length - 1]).toBeDisabled();
  });

  it("deve navegar para página anterior", async () => {
    const onPageChange = vi.fn();
    render(<Pagination page={2} totalPages={3} onPageChange={onPageChange} />);

    const buttons = screen.getAllByRole("button");
    await userEvent.click(buttons[0]);

    expect(onPageChange).toHaveBeenCalledWith(1);
  });

  it("deve navegar para próxima página", async () => {
    const onPageChange = vi.fn();
    render(<Pagination page={2} totalPages={3} onPageChange={onPageChange} />);

    const buttons = screen.getAllByRole("button");
    await userEvent.click(buttons[buttons.length - 1]);

    expect(onPageChange).toHaveBeenCalledWith(3);
  });

  it("deve exibir reticências para muitas páginas", () => {
    render(<Pagination page={5} totalPages={10} onPageChange={vi.fn()} />);
    const dots = screen.getAllByText("...");
    expect(dots.length).toBeGreaterThanOrEqual(1);
  });
});
