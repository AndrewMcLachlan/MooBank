import { describe, it, expect, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { ReportTypeSelector } from "components/ReportTypeSelector";

describe("ReportTypeSelector", () => {
    it("highlights Income as active when value is Credit", () => {
        render(<ReportTypeSelector value="Credit" />);
        expect(screen.getByRole("button", { name: "Income" })).toHaveClass("btn-primary");
        expect(screen.getByRole("button", { name: "Expense" })).toHaveClass("btn-outline-primary");
    });

    it("highlights Expense as active when value is Debit", () => {
        render(<ReportTypeSelector value="Debit" />);
        expect(screen.getByRole("button", { name: "Expense" })).toHaveClass("btn-primary");
        expect(screen.getByRole("button", { name: "Income" })).toHaveClass("btn-outline-primary");
    });

    it("neither button is active when value is undefined", () => {
        render(<ReportTypeSelector />);
        expect(screen.getByRole("button", { name: "Income" })).toHaveClass("btn-outline-primary");
        expect(screen.getByRole("button", { name: "Expense" })).toHaveClass("btn-outline-primary");
    });

    it("calls onChange with Credit when Income is clicked", async () => {
        const user = userEvent.setup();
        const onChange = vi.fn();
        render(<ReportTypeSelector value="Debit" onChange={onChange} />);
        await user.click(screen.getByRole("button", { name: "Income" }));
        expect(onChange).toHaveBeenCalledTimes(1);
        expect(onChange).toHaveBeenCalledWith("Credit");
    });

    it("calls onChange with Debit when Expense is clicked", async () => {
        const user = userEvent.setup();
        const onChange = vi.fn();
        render(<ReportTypeSelector value="Credit" onChange={onChange} />);
        await user.click(screen.getByRole("button", { name: "Expense" }));
        expect(onChange).toHaveBeenCalledTimes(1);
        expect(onChange).toHaveBeenCalledWith("Debit");
    });

    it("does not call onChange when clicking the already-selected option", async () => {
        const user = userEvent.setup();
        const onChange = vi.fn();
        render(<ReportTypeSelector value="Credit" onChange={onChange} />);
        await user.click(screen.getByRole("button", { name: "Income" }));
        expect(onChange).not.toHaveBeenCalled();
    });
});
