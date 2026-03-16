import { describe, it, expect } from "vitest";
import { cn } from "../../lib/utils";

describe("cn", () => {
  it("deve combinar classes simples", () => {
    expect(cn("foo", "bar")).toBe("foo bar");
  });

  it("deve lidar com valores condicionais", () => {
    expect(cn("base", false && "hidden", "visible")).toBe("base visible");
  });

  it("deve mesclar classes Tailwind conflitantes", () => {
    expect(cn("px-4", "px-6")).toBe("px-6");
  });

  it("deve lidar com undefined e null", () => {
    expect(cn("base", undefined, null, "end")).toBe("base end");
  });

  it("deve lidar com string vazia", () => {
    expect(cn("", "foo")).toBe("foo");
  });

  it("deve lidar com arrays", () => {
    expect(cn(["foo", "bar"])).toBe("foo bar");
  });
});
