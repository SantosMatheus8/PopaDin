import { api } from "./api";
import type {
  CreateGoalRequest,
  UpdateGoalRequest,
  GoalResponse,
  PaginatedResult,
  ListGoalsRequest,
} from "../types";

export const goalService = {
  create: async (data: CreateGoalRequest): Promise<GoalResponse> => {
    const response = await api.post<GoalResponse>("/goal", data);
    return response.data;
  },

  list: async (params?: ListGoalsRequest): Promise<PaginatedResult<GoalResponse>> => {
    const response = await api.get<PaginatedResult<GoalResponse>>("/goal", { params });
    return response.data;
  },

  findById: async (goalId: number): Promise<GoalResponse> => {
    const response = await api.get<GoalResponse>(`/goal/${goalId}`);
    return response.data;
  },

  update: async (goalId: number, data: UpdateGoalRequest): Promise<GoalResponse> => {
    const response = await api.put<GoalResponse>(`/goal/${goalId}`, data);
    return response.data;
  },

  delete: async (goalId: number): Promise<void> => {
    await api.delete(`/goal/${goalId}`);
  },

  finish: async (goalId: number): Promise<void> => {
    await api.patch(`/goal/${goalId}`);
  },
};
