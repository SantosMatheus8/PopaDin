import { api } from "./api";
import type { LoginRequest, LoginResponse, UserResponse } from "../types";

export const authService = {
  login: async (data: LoginRequest): Promise<LoginResponse> => {
    const response = await api.post<LoginResponse>("/auth/login", data);
    return response.data;
  },

  getProfile: async (): Promise<UserResponse> => {
    const response = await api.get<UserResponse>("/auth/profile");
    return response.data;
  },
};
