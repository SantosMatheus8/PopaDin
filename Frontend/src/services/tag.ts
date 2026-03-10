import { api } from "./api";
import type {
  CreateTagRequest,
  UpdateTagRequest,
  TagResponse,
  PaginatedResult,
  ListTagsRequest,
} from "../types";

export const tagService = {
  create: async (data: CreateTagRequest): Promise<TagResponse> => {
    const response = await api.post<TagResponse>("/tag", data);
    return response.data;
  },

  list: async (params?: ListTagsRequest): Promise<PaginatedResult<TagResponse>> => {
    const response = await api.get<PaginatedResult<TagResponse>>("/tag", { params });
    return response.data;
  },

  findById: async (tagId: number): Promise<TagResponse> => {
    const response = await api.get<TagResponse>(`/tag/${tagId}`);
    return response.data;
  },

  update: async (tagId: number, data: UpdateTagRequest): Promise<TagResponse> => {
    const response = await api.put<TagResponse>(`/tag/${tagId}`, data);
    return response.data;
  },

  delete: async (tagId: number): Promise<void> => {
    await api.delete(`/tag/${tagId}`);
  },
};
