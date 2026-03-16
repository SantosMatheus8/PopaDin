import { describe, it, expect, vi, beforeEach } from "vitest";
import { authService } from "../../services/auth";
import { api } from "../../services/api";

vi.mock("../../services/api", () => ({
  api: {
    post: vi.fn(),
    get: vi.fn(),
  },
}));

describe("authService", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe("login", () => {
    it("deve fazer POST para /auth/login", async () => {
      const mockResponse = { data: { access_token: "token123" } };
      vi.mocked(api.post).mockResolvedValue(mockResponse);

      const result = await authService.login({ email: "user@test.com", password: "123456" });

      expect(api.post).toHaveBeenCalledWith("/auth/login", {
        email: "user@test.com",
        password: "123456",
      });
      expect(result).toEqual({ access_token: "token123" });
    });
  });

  describe("getProfile", () => {
    it("deve fazer GET para /auth/profile", async () => {
      const mockUser = { id: 1, name: "João", email: "joao@test.com", balance: 100 };
      vi.mocked(api.get).mockResolvedValue({ data: mockUser });

      const result = await authService.getProfile();

      expect(api.get).toHaveBeenCalledWith("/auth/profile");
      expect(result).toEqual(mockUser);
    });
  });
});
