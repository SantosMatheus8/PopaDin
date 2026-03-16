import { describe, it, expect, vi, beforeEach } from "vitest";
import axios from "axios";

vi.mock("axios", () => {
  const mockAxiosInstance = {
    get: vi.fn(),
    patch: vi.fn(),
    delete: vi.fn(),
    interceptors: {
      request: { use: vi.fn() },
      response: { use: vi.fn() },
    },
  };
  return {
    default: {
      create: vi.fn(() => mockAxiosInstance),
      isAxiosError: vi.fn(),
    },
  };
});

describe("notificationService", () => {
  let notificationService: typeof import("../../services/notification").notificationService;
  let mockApi: any;

  beforeEach(async () => {
    vi.clearAllMocks();
    mockApi = axios.create();
    const mod = await import("../../services/notification");
    notificationService = mod.notificationService;
  });

  it("deve listar notificações com paginação", async () => {
    const mockData = { data: [], total: 0, page: 1, limit: 20 };
    mockApi.get.mockResolvedValue({ data: mockData });

    const result = await notificationService.list(1, 20);

    expect(mockApi.get).toHaveBeenCalledWith("/notifications", { params: { page: 1, limit: 20 } });
    expect(result).toEqual(mockData);
  });

  it("deve marcar notificação como lida", async () => {
    const mockNotification = { _id: "n1", read: true };
    mockApi.patch.mockResolvedValue({ data: mockNotification });

    const result = await notificationService.markAsRead("n1");

    expect(mockApi.patch).toHaveBeenCalledWith("/notifications/n1/read");
    expect(result).toEqual(mockNotification);
  });

  it("deve marcar todas como lidas", async () => {
    mockApi.patch.mockResolvedValue({ data: { modifiedCount: 5 } });

    const result = await notificationService.markAllAsRead();

    expect(mockApi.patch).toHaveBeenCalledWith("/notifications/read-all");
    expect(result.modifiedCount).toBe(5);
  });

  it("deve retornar contagem de não lidas", async () => {
    mockApi.get.mockResolvedValue({ data: { count: 3 } });

    const result = await notificationService.getUnreadCount();

    expect(mockApi.get).toHaveBeenCalledWith("/notifications/unread-count");
    expect(result).toBe(3);
  });

  it("deve deletar todas notificações", async () => {
    mockApi.delete.mockResolvedValue({ data: { deletedCount: 10 } });

    const result = await notificationService.deleteAll();

    expect(mockApi.delete).toHaveBeenCalledWith("/notifications");
    expect(result.deletedCount).toBe(10);
  });
});
