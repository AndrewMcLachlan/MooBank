import { describe, it, expect } from "vitest";
import { render, screen } from "@testing-library/react";
import { AccountTypeBadge } from "components/AccountTypeBadge";

describe("AccountTypeBadge", () => {
    it.each([
        ["Transaction", "bg-blue"],
        ["Savings", "bg-emerald"],
        ["Credit", "bg-orange"],
        ["Mortgage", "bg-rose"],
        ["Loan", "bg-pink"],
        ["Superannuation", "bg-indigo"],
        ["Investment", "bg-teal"],
        ["Broker", "bg-purple"],
        ["Asset", "bg-amber"],
        ["Shares", "bg-cyan"],
        ["Virtual", "bg-slate"],
        ["Reserved Sum", "bg-neutral"],
    ])("renders %s with hue class %s", (type, hueClass) => {
        render(<AccountTypeBadge type={type} />);
        const badge = screen.getByText(type);
        expect(badge).toHaveClass(hueClass);
    });

    it("falls back to the secondary badge for an unrecognised type", () => {
        render(<AccountTypeBadge type="SomethingUnknown" />);
        const badge = screen.getByText("SomethingUnknown");
        expect(badge).toHaveClass("bg-secondary");
    });

    it("renders nothing when type is null", () => {
        const { container } = render(<AccountTypeBadge type={null} />);
        expect(container).toBeEmptyDOMElement();
    });

    it("renders nothing when type is undefined", () => {
        const { container } = render(<AccountTypeBadge type={undefined} />);
        expect(container).toBeEmptyDOMElement();
    });

    it("renders nothing when type is an empty string", () => {
        const { container } = render(<AccountTypeBadge type="" />);
        expect(container).toBeEmptyDOMElement();
    });
});
