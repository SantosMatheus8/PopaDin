import { describe, it, expect } from "vitest";
import { render, screen } from "@testing-library/react";
import { Card } from "../../components/Card/index";

describe("Card", () => {
  it("deve renderizar filhos", () => {
    render(<Card><p>Card content</p></Card>);
    expect(screen.getByText("Card content")).toBeInTheDocument();
  });

  it("deve aplicar className adicional", () => {
    const { container } = render(<Card className="custom-class"><p>Test</p></Card>);
    expect(container.firstChild).toHaveClass("custom-class");
  });

  it("deve ter classes base de estilo", () => {
    const { container } = render(<Card><p>Test</p></Card>);
    expect(container.firstChild).toHaveClass("rounded-xl");
  });
});
