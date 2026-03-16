import { describe, it, expect, vi, beforeEach } from "vitest";
import { dashboardService } from "../../services/dashboard";
import { api } from "../../services/api";

vi.mock("../../services/api", () => ({
  api: {
    get: vi.fn(),
  },
}));

describe("dashboardService", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("deve buscar dashboard sem parâmetros", async () => {
    const mockData = {
      summary: { totalDeposits: 1000, totalOutflows: 500, balance: 500, recordCount: 10 },
      budgets: [],
      spendingByTag: [],
      latestRecords: [],
      topDeposits: [],
      topOutflows: [],
    };
    vi.mocked(api.get).mockResolvedValue({ data: mockData });

    const result = await dashboardService.get();

    expect(api.get).toHaveBeenCalledWith("/dashboard", { params: undefined });
    expect(result.summary.balance).toBe(500);
  });

  it("deve buscar dashboard com filtro de datas", async () => {
    vi.mocked(api.get).mockResolvedValue({ data: {} });

    await dashboardService.get({ startDate: "2024-01-01", endDate: "2024-12-31" });

    expect(api.get).toHaveBeenCalledWith("/dashboard", {
      params: { startDate: "2024-01-01", endDate: "2024-12-31" },
    });
  });
});
