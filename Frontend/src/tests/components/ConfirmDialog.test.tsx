import { describe, it, expect, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { ConfirmDialog } from "../../components/ConfirmDialog/index";

describe("ConfirmDialog", () => {
  const defaultProps = {
    isOpen: true,
    onClose: vi.fn(),
    onConfirm: vi.fn(),
    title: "Confirmar exclusão",
    message: "Deseja realmente excluir?",
  };

  it("deve renderizar título e mensagem", () => {
    render(<ConfirmDialog {...defaultProps} />);
    expect(screen.getByText("Confirmar exclusão")).toBeInTheDocument();
    expect(screen.getByText("Deseja realmente excluir?")).toBeInTheDocument();
  });

  it("deve exibir botão Cancelar e Confirmar", () => {
    render(<ConfirmDialog {...defaultProps} />);
    expect(screen.getByText("Cancelar")).toBeInTheDocument();
    expect(screen.getByText("Confirmar")).toBeInTheDocument();
  });

  it("deve exibir label customizado no botão de confirmação", () => {
    render(<ConfirmDialog {...defaultProps} confirmLabel="Excluir" />);
    expect(screen.getByText("Excluir")).toBeInTheDocument();
  });

  it("deve chamar onClose ao clicar em Cancelar", async () => {
    const onClose = vi.fn();
    render(<ConfirmDialog {...defaultProps} onClose={onClose} />);

    await userEvent.click(screen.getByText("Cancelar"));

    expect(onClose).toHaveBeenCalledTimes(1);
  });

  it("deve chamar onConfirm ao clicar em Confirmar", async () => {
    const onConfirm = vi.fn();
    render(<ConfirmDialog {...defaultProps} onConfirm={onConfirm} />);

    await userEvent.click(screen.getByText("Confirmar"));

    expect(onConfirm).toHaveBeenCalledTimes(1);
  });

  it("deve exibir 'Aguarde...' quando isLoading=true", () => {
    render(<ConfirmDialog {...defaultProps} isLoading={true} />);
    expect(screen.getByText("Aguarde...")).toBeInTheDocument();
  });

  it("deve desabilitar botões quando isLoading=true", () => {
    render(<ConfirmDialog {...defaultProps} isLoading={true} />);
    expect(screen.getByText("Cancelar").closest("button")).toBeDisabled();
    expect(screen.getByText("Aguarde...").closest("button")).toBeDisabled();
  });
});
