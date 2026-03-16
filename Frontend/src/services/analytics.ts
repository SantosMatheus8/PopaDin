import axios from "axios";

const ANALYTICS_BASE_URL =
  import.meta.env.VITE_ANALYTICS_URL || "http://localhost:3002";

const analyticsApi = axios.create({
  baseURL: ANALYTICS_BASE_URL,
});

analyticsApi.interceptors.request.use((config) => {
  const token = localStorage.getItem("access_token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

analyticsApi.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem("access_token");
      window.location.href = "/login";
    }
    return Promise.reject(error);
  }
);

export interface InsightPeriod {
  start: string;
  end: string;
}

export interface InsightResponse {
  _id: string;
  userId: number;
  type: string;
  title: string;
  message: string;
  severity: "info" | "warning" | "critical";
  data: Record<string, unknown>;
  period: InsightPeriod;
  expiresAt: string;
  createdAt: string;
}

export interface InsightListResponse {
  data: InsightResponse[];
  total: number;
  page: number;
  limit: number;
}

export interface ForecastData {
  currentBalance: number;
  forecast: { months: number; projected: number }[];
  goesNegativeInMonths: number | null;
  monthlyRecurringIncome: number;
  monthlyRecurringExpenses: number;
  averageOneTimeExpenses: number;
}

export interface SpendingTrendData {
  tagId: number;
  tagName: string;
  currentMonth: number;
  previousMonth: number;
  changePercent: number;
  direction: "up" | "down";
}

export interface MonthlyComparisonData {
  currentMonth: string;
  previousMonth: string;
  deposits: { current: number; previous: number; changePercent: number };
  outflows: { current: number; previous: number; changePercent: number };
  netBalance: { current: number; previous: number; changePercent: number };
}

export interface AnomalyData {
  recordId: string;
  recordName: string;
  value: number;
  tagName: string;
  average: number;
  standardDeviation: number;
  deviationsAbove: number;
}

export const analyticsService = {
  async getInsights(
    type?: string,
    page: number = 1,
    limit: number = 20
  ): Promise<InsightListResponse> {
    const params: Record<string, string | number> = { page, limit };
    if (type) params.type = type;
    const { data } = await analyticsApi.get("/analytics/insights", { params });
    return data;
  },

  async getLatestInsights(): Promise<InsightResponse[]> {
    const { data } = await analyticsApi.get("/analytics/insights/latest");
    return data;
  },

  async getForecast(): Promise<InsightResponse | null> {
    const { data } = await analyticsApi.get("/analytics/insights/forecast");
    return data;
  },

  async refreshInsights(): Promise<void> {
    await analyticsApi.post("/analytics/insights/refresh");
  },
};
