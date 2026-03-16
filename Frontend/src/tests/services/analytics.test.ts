import { describe, it, expect, vi, beforeEach } from "vitest";
import axios from "axios";

vi.mock("axios", () => {
  const mockAxiosInstance = {
    get: vi.fn(),
    post: vi.fn(),
    interceptors: {
      request: { use: vi.fn() },
      response: { use: vi.fn() },
    },
  };
  return {
    default: {
      create: vi.fn(() => mockAxiosInstance),
    },
  };
});

describe("analyticsService", () => {
  let analyticsService: typeof import("../../services/analytics").analyticsService;
  let mockApi: any;

  beforeEach(async () => {
    vi.clearAllMocks();
    mockApi = axios.create();
    const mod = await import("../../services/analytics");
    analyticsService = mod.analyticsService;
  });

  it("deve buscar insights sem filtro de tipo", async () => {
    const mockData = { data: [], total: 0, page: 1, limit: 20 };
    mockApi.get.mockResolvedValue({ data: mockData });

    const result = await analyticsService.getInsights();

    expect(mockApi.get).toHaveBeenCalledWith("/analytics/insights", {
      params: { page: 1, limit: 20 },
    });
    expect(result).toEqual(mockData);
  });

  it("deve buscar insights com filtro de tipo", async () => {
    const mockData = { data: [], total: 0, page: 1, limit: 20 };
    mockApi.get.mockResolvedValue({ data: mockData });

    await analyticsService.getInsights("spending_trend", 2, 10);

    expect(mockApi.get).toHaveBeenCalledWith("/analytics/insights", {
      params: { page: 2, limit: 10, type: "spending_trend" },
    });
  });

  it("deve buscar últimos insights", async () => {
    const mockInsights = [{ _id: "i1", type: "spending_trend" }];
    mockApi.get.mockResolvedValue({ data: mockInsights });

    const result = await analyticsService.getLatestInsights();

    expect(mockApi.get).toHaveBeenCalledWith("/analytics/insights/latest");
    expect(result).toEqual(mockInsights);
  });

  it("deve buscar forecast", async () => {
    const mockForecast = { _id: "f1", type: "balance_forecast" };
    mockApi.get.mockResolvedValue({ data: mockForecast });

    const result = await analyticsService.getForecast();

    expect(mockApi.get).toHaveBeenCalledWith("/analytics/insights/forecast");
    expect(result).toEqual(mockForecast);
  });

  it("deve atualizar insights com POST", async () => {
    mockApi.post.mockResolvedValue({});

    await analyticsService.refreshInsights();

    expect(mockApi.post).toHaveBeenCalledWith("/analytics/insights/refresh");
  });
});
