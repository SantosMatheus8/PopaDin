import { describe, it, expect, vi, beforeEach } from "vitest";
import { alertService } from "../../services/alert";
import { api } from "../../services/api";

vi.mock("../../services/api", () => ({
  api: {
    post: vi.fn(),
    get: vi.fn(),
    patch: vi.fn(),
    delete: vi.fn(),
  },
}));

describe("alertService", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("deve criar alerta com POST /alert", async () => {
    const data = { type: 0, threshold: 500 };
    vi.mocked(api.post).mockResolvedValue({ data: { id: "a1", ...data } });

    const result = await alertService.create(data);

    expect(api.post).toHaveBeenCalledWith("/alert", data);
    expect(result.id).toBe("a1");
  });

  it("deve listar alertas com GET /alert", async () => {
    const mockAlerts = [{ id: "a1", type: "BALANCE_BELOW", threshold: 500 }];
    vi.mocked(api.get).mockResolvedValue({ data: mockAlerts });

    const result = await alertService.list();

    expect(api.get).toHaveBeenCalledWith("/alert");
    expect(result).toEqual(mockAlerts);
  });

  it("deve alternar alerta com PATCH /alert/:id", async () => {
    vi.mocked(api.patch).mockResolvedValue({});

    await alertService.toggle("a1", { active: false });

    expect(api.patch).toHaveBeenCalledWith("/alert/a1", { active: false });
  });

  it("deve deletar alerta com DELETE /alert/:id", async () => {
    vi.mocked(api.delete).mockResolvedValue({});

    await alertService.delete("a1");

    expect(api.delete).toHaveBeenCalledWith("/alert/a1");
  });
});
