import { describe, it, expect, vi, beforeEach } from "vitest";
import { userService } from "../../services/user";
import { api } from "../../services/api";

vi.mock("../../services/api", () => ({
  api: {
    post: vi.fn(),
    get: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  },
}));

describe("userService", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("deve criar usuário com POST /user", async () => {
    const data = { name: "João", email: "joao@test.com", password: "123456" };
    const mockResponse = { data: { id: 1, ...data } };
    vi.mocked(api.post).mockResolvedValue(mockResponse);

    const result = await userService.create(data);

    expect(api.post).toHaveBeenCalledWith("/user", data);
    expect(result.id).toBe(1);
  });

  it("deve listar usuários com GET /user", async () => {
    const mockData = { lines: [], page: 1, pageSize: 10, totalItems: 0, totalPages: 0 };
    vi.mocked(api.get).mockResolvedValue({ data: mockData });

    const result = await userService.list({ page: 1 });

    expect(api.get).toHaveBeenCalledWith("/user", { params: { page: 1 } });
    expect(result).toEqual(mockData);
  });

  it("deve buscar usuário por id com GET /user/:id", async () => {
    const mockUser = { id: 1, name: "João" };
    vi.mocked(api.get).mockResolvedValue({ data: mockUser });

    const result = await userService.findById(1);

    expect(api.get).toHaveBeenCalledWith("/user/1");
    expect(result).toEqual(mockUser);
  });

  it("deve atualizar usuário com PUT /user/:id", async () => {
    const data = { name: "João Atualizado", balance: 200 };
    vi.mocked(api.put).mockResolvedValue({ data: { id: 1, ...data } });

    const result = await userService.update(1, data as any);

    expect(api.put).toHaveBeenCalledWith("/user/1", data);
    expect(result.name).toBe("João Atualizado");
  });

  it("deve deletar usuário com DELETE /user/:id", async () => {
    vi.mocked(api.delete).mockResolvedValue({});

    await userService.delete(1);

    expect(api.delete).toHaveBeenCalledWith("/user/1");
  });
});
