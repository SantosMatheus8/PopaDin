import { api } from "./api";
import type {
  CreateBudgetRequest,
  UpdateBudgetRequest,
  BudgetResponse,
  PaginatedResult,
  ListBudgetsRequest,
} from "../types";

export const budgetService = {
  create: async (data: CreateBudgetRequest): Promise<BudgetResponse> => {
    const response = await api.post<BudgetResponse>("/budget", data);
    return response.data;
  },

  list: async (params?: ListBudgetsRequest): Promise<PaginatedResult<BudgetResponse>> => {
    const response = await api.get<PaginatedResult<BudgetResponse>>("/budget", { params });
    return response.data;
  },

  findById: async (budgetId: number): Promise<BudgetResponse> => {
    const response = await api.get<BudgetResponse>(`/budget/${budgetId}`);
    return response.data;
  },

  update: async (budgetId: number, data: UpdateBudgetRequest): Promise<BudgetResponse> => {
    const response = await api.put<BudgetResponse>(`/budget/${budgetId}`, data);
    return response.data;
  },

  delete: async (budgetId: number): Promise<void> => {
    await api.delete(`/budget/${budgetId}`);
  },

  finish: async (budgetId: number): Promise<void> => {
    await api.patch(`/budget/${budgetId}`);
  },
};
