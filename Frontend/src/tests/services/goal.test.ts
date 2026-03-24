import { describe, it, expect, vi, beforeEach } from "vitest";
import { goalService } from "../../services/goal";
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

describe("goalService", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("deve criar meta com POST /goal", async () => {
    const data = { name: "Viagem", targetAmount: 5000 };
    vi.mocked(api.post).mockResolvedValue({ data: { id: 1, ...data } });

    const result = await goalService.create(data);

    expect(api.post).toHaveBeenCalledWith("/goal", data);
    expect(result.id).toBe(1);
  });

  it("deve listar metas com GET /goal", async () => {
    const mockData = { lines: [], page: 1, pageSize: 10, totalItems: 0, totalPages: 0 };
    vi.mocked(api.get).mockResolvedValue({ data: mockData });

    const result = await goalService.list();

    expect(api.get).toHaveBeenCalledWith("/goal", { params: undefined });
    expect(result).toEqual(mockData);
  });

  it("deve buscar meta por id com GET /goal/:id", async () => {
    vi.mocked(api.get).mockResolvedValue({ data: { id: 1, name: "Viagem" } });

    const result = await goalService.findById(1);

    expect(api.get).toHaveBeenCalledWith("/goal/1");
    expect(result.name).toBe("Viagem");
  });

  it("deve atualizar meta com PUT /goal/:id", async () => {
    const data = { name: "Viagem Europa", targetAmount: 10000 };
    vi.mocked(api.put).mockResolvedValue({ data: { id: 1, ...data } });

    const result = await goalService.update(1, data);

    expect(api.put).toHaveBeenCalledWith("/goal/1", data);
    expect(result.name).toBe("Viagem Europa");
  });

  it("deve deletar meta com DELETE /goal/:id", async () => {
    vi.mocked(api.delete).mockResolvedValue({});

    await goalService.delete(1);

    expect(api.delete).toHaveBeenCalledWith("/goal/1");
  });

  it("deve marcar meta como batida com PATCH /goal/:id", async () => {
    vi.mocked(api.patch).mockResolvedValue({});

    await goalService.finish(1);

    expect(api.patch).toHaveBeenCalledWith("/goal/1");
  });
});
