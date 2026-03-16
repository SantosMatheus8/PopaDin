import { describe, it, expect, vi, beforeEach } from "vitest";
import { renderHook } from "@testing-library/react";
import { useApiError } from "../../hooks/useApiError";
import toast from "react-hot-toast";
import axios, { AxiosError } from "axios";

vi.mock("react-hot-toast", () => ({
  default: {
    error: vi.fn(),
  },
}));

describe("useApiError", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("deve exibir mensagem de detail do AxiosError", () => {
    const { result } = renderHook(() => useApiError());
    const error = new AxiosError("fail");
    (error as any).response = { data: { detail: "Erro de validação" }, status: 400 };
    vi.spyOn(axios, "isAxiosError").mockReturnValue(true);

    result.current.handleError(error);

    expect(toast.error).toHaveBeenCalledWith("Erro de validação");
  });

  it("deve exibir mensagem de title do AxiosError", () => {
    const { result } = renderHook(() => useApiError());
    const error = new AxiosError("fail");
    (error as any).response = { data: { title: "Not Found" }, status: 404 };
    vi.spyOn(axios, "isAxiosError").mockReturnValue(true);

    result.current.handleError(error);

    expect(toast.error).toHaveBeenCalledWith("Not Found");
  });

  it("deve exibir mensagem de message do AxiosError", () => {
    const { result } = renderHook(() => useApiError());
    const error = new AxiosError("fail");
    (error as any).response = { data: { message: "Server error" }, status: 500 };
    vi.spyOn(axios, "isAxiosError").mockReturnValue(true);

    result.current.handleError(error);

    expect(toast.error).toHaveBeenCalledWith("Server error");
  });

  it("deve exibir fallback para AxiosError sem mensagem", () => {
    const { result } = renderHook(() => useApiError());
    const error = new AxiosError("fail");
    (error as any).response = { data: {}, status: 500 };
    vi.spyOn(axios, "isAxiosError").mockReturnValue(true);

    result.current.handleError(error);

    expect(toast.error).toHaveBeenCalledWith("Ocorreu um erro inesperado");
  });

  it("deve exibir fallback para erro genérico", () => {
    const { result } = renderHook(() => useApiError());
    vi.spyOn(axios, "isAxiosError").mockReturnValue(false);

    result.current.handleError(new Error("generic"));

    expect(toast.error).toHaveBeenCalledWith("Ocorreu um erro inesperado");
  });

  it("deve exibir fallback customizado", () => {
    const { result } = renderHook(() => useApiError());
    vi.spyOn(axios, "isAxiosError").mockReturnValue(false);

    result.current.handleError(new Error("fail"), "Erro customizado");

    expect(toast.error).toHaveBeenCalledWith("Erro customizado");
  });
});
