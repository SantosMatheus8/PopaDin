import { describe, it, expect, vi, beforeEach } from "vitest";
import { recordService } from "../../services/record";
import { api } from "../../services/api";

vi.mock("../../services/api", () => ({
  api: {
    post: vi.fn(),
    get: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  },
}));

describe("recordService", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("deve criar record com POST /record", async () => {
    const data = { name: "Salário", operation: 1, value: 5000, frequency: 0, tagIds: [] };
    vi.mocked(api.post).mockResolvedValue({ data: { id: "abc", ...data } });

    const result = await recordService.create(data as any);

    expect(api.post).toHaveBeenCalledWith("/record", data);
    expect(result.id).toBe("abc");
  });

  it("deve listar records com GET /record", async () => {
    const mockData = { lines: [], page: 1, pageSize: 10, totalItems: 0, totalPages: 0 };
    vi.mocked(api.get).mockResolvedValue({ data: mockData });

    const result = await recordService.list({ page: 1 });

    expect(api.get).toHaveBeenCalledWith("/record", { params: { page: 1 } });
    expect(result).toEqual(mockData);
  });

  it("deve buscar record por id com GET /record/:id", async () => {
    vi.mocked(api.get).mockResolvedValue({ data: { id: "abc", name: "Compra" } });

    const result = await recordService.findById("abc");

    expect(api.get).toHaveBeenCalledWith("/record/abc");
    expect(result.name).toBe("Compra");
  });

  it("deve atualizar record com PUT /record/:id", async () => {
    const data = { name: "Salário Atualizado", operation: 1, value: 6000, frequency: 0, tagIds: [] };
    vi.mocked(api.put).mockResolvedValue({ data: { id: "abc", ...data } });

    const result = await recordService.update("abc", data as any);

    expect(api.put).toHaveBeenCalledWith("/record/abc", data);
    expect(result.name).toBe("Salário Atualizado");
  });

  it("deve deletar record com DELETE /record/:id", async () => {
    vi.mocked(api.delete).mockResolvedValue({});

    await recordService.delete("abc");

    expect(api.delete).toHaveBeenCalledWith("/record/abc");
  });

  it("deve exportar records com POST /record/export", async () => {
    vi.mocked(api.post).mockResolvedValue({});

    await recordService.exportRecords({ startDate: "2024-01-01", endDate: "2024-12-31" });

    expect(api.post).toHaveBeenCalledWith("/record/export", {
      startDate: "2024-01-01",
      endDate: "2024-12-31",
    });
  });

  it("deve listar arquivos de exportação com GET /record/export/files", async () => {
    const mockFiles = [{ name: "export.pdf", url: "/files/export.pdf", size: 1024 }];
    vi.mocked(api.get).mockResolvedValue({ data: mockFiles });

    const result = await recordService.listExportFiles();

    expect(api.get).toHaveBeenCalledWith("/record/export/files");
    expect(result).toEqual(mockFiles);
  });

  it("deve baixar arquivo de exportação com GET /record/export/files/:name", async () => {
    const mockBlob = new Blob(["pdf content"]);
    vi.mocked(api.get).mockResolvedValue({ data: mockBlob });

    const result = await recordService.downloadExportFile("export.pdf");

    expect(api.get).toHaveBeenCalledWith("/record/export/files/export.pdf", {
      responseType: "blob",
    });
    expect(result).toBe(mockBlob);
  });
});
