import { api } from "./api";
import type { DashboardRequest, DashboardResponse } from "../types";

export const dashboardService = {
  get: async (params?: DashboardRequest): Promise<DashboardResponse> => {
    const response = await api.get<DashboardResponse>("/dashboard", { params });
    return response.data;
  },
};
