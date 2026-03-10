import { api } from "./api";
import type {
  CreateAlertRequest,
  AlertResponse,
  ToggleAlertRequest,
} from "../types";

export const alertService = {
  create: async (data: CreateAlertRequest): Promise<AlertResponse> => {
    const response = await api.post<AlertResponse>("/alert", data);
    return response.data;
  },

  list: async (): Promise<AlertResponse[]> => {
    const response = await api.get<AlertResponse[]>("/alert");
    return response.data;
  },

  toggle: async (id: string, data: ToggleAlertRequest): Promise<void> => {
    await api.patch(`/alert/${id}`, data);
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`/alert/${id}`);
  },
};
