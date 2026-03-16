import { describe, it, expect, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { Modal } from "../../components/Modal/index";

describe("Modal", () => {
  it("deve renderizar quando isOpen=true", () => {
    render(
      <Modal isOpen={true} onClose={vi.fn()} title="Título">
        <p>Conteúdo</p>
      </Modal>
    );
    expect(screen.getByText("Título")).toBeInTheDocument();
    expect(screen.getByText("Conteúdo")).toBeInTheDocument();
  });

  it("não deve renderizar quando isOpen=false", () => {
    render(
      <Modal isOpen={false} onClose={vi.fn()} title="Título">
        <p>Conteúdo</p>
      </Modal>
    );
    expect(screen.queryByText("Título")).not.toBeInTheDocument();
  });

  it("deve chamar onClose ao clicar no botão fechar", async () => {
    const onClose = vi.fn();
    render(
      <Modal isOpen={true} onClose={onClose} title="Título">
        <p>Conteúdo</p>
      </Modal>
    );

    const closeBtn = screen.getByRole("button");
    await userEvent.click(closeBtn);

    expect(onClose).toHaveBeenCalledTimes(1);
  });

  it("deve chamar onClose ao pressionar Escape", async () => {
    const onClose = vi.fn();
    render(
      <Modal isOpen={true} onClose={onClose} title="Título">
        <p>Conteúdo</p>
      </Modal>
    );

    await userEvent.keyboard("{Escape}");

    expect(onClose).toHaveBeenCalledTimes(1);
  });

  it("deve bloquear scroll do body quando aberto", () => {
    render(
      <Modal isOpen={true} onClose={vi.fn()} title="Título">
        <p>Conteúdo</p>
      </Modal>
    );
    expect(document.body.style.overflow).toBe("hidden");
  });
});
