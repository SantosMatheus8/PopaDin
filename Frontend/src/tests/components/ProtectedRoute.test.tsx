import { describe, it, expect, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { ProtectedRoute } from "../../components/ProtectedRoute/index";

const mockUseAuth = vi.fn();

vi.mock("../../hooks/useAuth", () => ({
  useAuth: () => mockUseAuth(),
}));

describe("ProtectedRoute", () => {
  it("deve renderizar children quando autenticado", () => {
    mockUseAuth.mockReturnValue({ isAuthenticated: true, isLoading: false });

    render(
      <MemoryRouter>
        <ProtectedRoute>
          <p>Conteúdo protegido</p>
        </ProtectedRoute>
      </MemoryRouter>
    );

    expect(screen.getByText("Conteúdo protegido")).toBeInTheDocument();
  });

  it("deve exibir spinner durante carregamento", () => {
    mockUseAuth.mockReturnValue({ isAuthenticated: false, isLoading: true });

    const { container } = render(
      <MemoryRouter>
        <ProtectedRoute>
          <p>Conteúdo</p>
        </ProtectedRoute>
      </MemoryRouter>
    );

    expect(container.querySelector(".animate-spin")).toBeInTheDocument();
    expect(screen.queryByText("Conteúdo")).not.toBeInTheDocument();
  });

  it("deve redirecionar para /login quando não autenticado", () => {
    mockUseAuth.mockReturnValue({ isAuthenticated: false, isLoading: false });

    render(
      <MemoryRouter>
        <ProtectedRoute>
          <p>Conteúdo</p>
        </ProtectedRoute>
      </MemoryRouter>
    );

    expect(screen.queryByText("Conteúdo")).not.toBeInTheDocument();
  });
});
