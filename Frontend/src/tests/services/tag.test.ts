import { describe, it, expect, vi, beforeEach } from "vitest";
import { tagService } from "../../services/tag";
import { api } from "../../services/api";

vi.mock("../../services/api", () => ({
  api: {
    post: vi.fn(),
    get: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  },
}));

describe("tagService", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("deve criar tag com POST /tag", async () => {
    const data = { name: "Alimentação" };
    vi.mocked(api.post).mockResolvedValue({ data: { id: 1, ...data } });

    const result = await tagService.create(data);

    expect(api.post).toHaveBeenCalledWith("/tag", data);
    expect(result.id).toBe(1);
  });

  it("deve listar tags com GET /tag", async () => {
    const mockData = { lines: [], page: 1, pageSize: 10, totalItems: 0, totalPages: 0 };
    vi.mocked(api.get).mockResolvedValue({ data: mockData });

    const result = await tagService.list();

    expect(api.get).toHaveBeenCalledWith("/tag", { params: undefined });
    expect(result).toEqual(mockData);
  });

  it("deve buscar tag por id com GET /tag/:id", async () => {
    vi.mocked(api.get).mockResolvedValue({ data: { id: 5, name: "Saúde" } });

    const result = await tagService.findById(5);

    expect(api.get).toHaveBeenCalledWith("/tag/5");
    expect(result.name).toBe("Saúde");
  });

  it("deve atualizar tag com PUT /tag/:id", async () => {
    const data = { name: "Transporte" };
    vi.mocked(api.put).mockResolvedValue({ data: { id: 1, ...data } });

    const result = await tagService.update(1, data as any);

    expect(api.put).toHaveBeenCalledWith("/tag/1", data);
    expect(result.name).toBe("Transporte");
  });

  it("deve deletar tag com DELETE /tag/:id", async () => {
    vi.mocked(api.delete).mockResolvedValue({});

    await tagService.delete(1);

    expect(api.delete).toHaveBeenCalledWith("/tag/1");
  });
});
