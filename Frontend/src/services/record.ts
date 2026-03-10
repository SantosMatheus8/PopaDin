import { api } from "./api";
import type {
  CreateRecordRequest,
  UpdateRecordRequest,
  RecordResponse,
  PaginatedResult,
  ListRecordsRequest,
  ExportRecordsRequest,
  ExportFileResponse,
} from "../types";

export const recordService = {
  create: async (data: CreateRecordRequest): Promise<RecordResponse> => {
    const response = await api.post<RecordResponse>("/record", data);
    return response.data;
  },

  list: async (params?: ListRecordsRequest): Promise<PaginatedResult<RecordResponse>> => {
    const response = await api.get<PaginatedResult<RecordResponse>>("/record", { params });
    return response.data;
  },

  findById: async (recordId: string): Promise<RecordResponse> => {
    const response = await api.get<RecordResponse>(`/record/${recordId}`);
    return response.data;
  },

  update: async (recordId: string, data: UpdateRecordRequest): Promise<RecordResponse> => {
    const response = await api.put<RecordResponse>(`/record/${recordId}`, data);
    return response.data;
  },

  delete: async (recordId: string): Promise<void> => {
    await api.delete(`/record/${recordId}`);
  },

  exportRecords: async (data: ExportRecordsRequest): Promise<void> => {
    await api.post("/record/export", data);
  },

  listExportFiles: async (): Promise<ExportFileResponse[]> => {
    const response = await api.get<ExportFileResponse[]>("/record/export/files");
    return response.data;
  },

  downloadExportFile: async (fileName: string): Promise<Blob> => {
    const response = await api.get(`/record/export/files/${fileName}`, {
      responseType: "blob",
    });
    return response.data;
  },
};
