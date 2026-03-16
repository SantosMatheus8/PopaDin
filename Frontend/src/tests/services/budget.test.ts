import { describe, it, expect, vi, beforeEach } from "vitest";
import { budgetService } from "../../services/budget";
import { api } from "../../services/api";

vi.mock("../../services/api", () => ({
  api: {
    post: vi.fn(),
    get: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
    patch: vi.fn(),
  },
}));

describe("budgetService", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("deve criar budget com POST /budget", async () => {
    const data = { name: "Viagem", goal: 5000 };
    vi.mocked(api.post).mockResolvedValue({ data: { id: 1, ...data } });

    const result = await budgetService.create(data);

    expect(api.post).toHaveBeenCalledWith("/budget", data);
    expect(result.id).toBe(1);
  });

  it("deve listar budgets com GET /budget", async () => {
    const mockData = { lines: [], page: 1, pageSize: 10, totalItems: 0, totalPages: 0 };
    vi.mocked(api.get).mockResolvedValue({ data: mockData });

    const result = await budgetService.list();

    expect(api.get).toHaveBeenCalledWith("/budget", { params: undefined });
    expect(result).toEqual(mockData);
  });

  it("deve buscar budget por id com GET /budget/:id", async () => {
    vi.mocked(api.get).mockResolvedValue({ data: { id: 1, name: "Viagem" } });

    const result = await budgetService.findById(1);

    expect(api.get).toHaveBeenCalledWith("/budget/1");
    expect(result.name).toBe("Viagem");
  });

  it("deve atualizar budget com PUT /budget/:id", async () => {
    const data = { name: "Viagem Europa", goal: 10000 };
    vi.mocked(api.put).mockResolvedValue({ data: { id: 1, ...data } });

    const result = await budgetService.update(1, data);

    expect(api.put).toHaveBeenCalledWith("/budget/1", data);
    expect(result.name).toBe("Viagem Europa");
  });

  it("deve deletar budget com DELETE /budget/:id", async () => {
    vi.mocked(api.delete).mockResolvedValue({});

    await budgetService.delete(1);

    expect(api.delete).toHaveBeenCalledWith("/budget/1");
  });

  it("deve finalizar budget com PATCH /budget/:id", async () => {
    vi.mocked(api.patch).mockResolvedValue({});

    await budgetService.finish(1);

    expect(api.patch).toHaveBeenCalledWith("/budget/1");
  });
});
