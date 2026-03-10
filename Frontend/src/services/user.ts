import { api } from "./api";
import type {
  CreateUserRequest,
  UpdateUserRequest,
  UserResponse,
  PaginatedResult,
  ListUsersRequest,
} from "../types";

export const userService = {
  create: async (data: CreateUserRequest): Promise<UserResponse> => {
    const response = await api.post<UserResponse>("/user", data);
    return response.data;
  },

  list: async (params?: ListUsersRequest): Promise<PaginatedResult<UserResponse>> => {
    const response = await api.get<PaginatedResult<UserResponse>>("/user", { params });
    return response.data;
  },

  findById: async (userId: number): Promise<UserResponse> => {
    const response = await api.get<UserResponse>(`/user/${userId}`);
    return response.data;
  },

  update: async (userId: number, data: UpdateUserRequest): Promise<UserResponse> => {
    const response = await api.put<UserResponse>(`/user/${userId}`, data);
    return response.data;
  },

  delete: async (userId: number): Promise<void> => {
    await api.delete(`/user/${userId}`);
  },
};
