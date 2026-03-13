import axios from "axios";
import type { NotificationListResponse, NotificationResponse } from "../types";

const NOTIFICATION_BASE_URL =
  import.meta.env.VITE_NOTIFICATION_URL || "http://localhost:3001";

const notificationApi = axios.create({
  baseURL: NOTIFICATION_BASE_URL,
});

notificationApi.interceptors.request.use((config) => {
  const token = localStorage.getItem("access_token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export const notificationService = {
  list: async (
    page = 1,
    limit = 20
  ): Promise<NotificationListResponse> => {
    const response = await notificationApi.get<NotificationListResponse>(
      "/notifications",
      { params: { page, limit } }
    );
    return response.data;
  },

  markAsRead: async (id: string): Promise<NotificationResponse> => {
    const response = await notificationApi.patch<NotificationResponse>(
      `/notifications/${id}/read`
    );
    return response.data;
  },

  markAllAsRead: async (): Promise<{ modifiedCount: number }> => {
    const response = await notificationApi.patch<{ modifiedCount: number }>(
      "/notifications/read-all"
    );
    return response.data;
  },

  getUnreadCount: async (): Promise<number> => {
    const response = await notificationApi.get<{ count: number }>(
      "/notifications/unread-count"
    );
    return response.data.count;
  },

  deleteAll: async (): Promise<{ deletedCount: number }> => {
    const response = await notificationApi.delete<{ deletedCount: number }>(
      "/notifications"
    );
    return response.data;
  },
};
