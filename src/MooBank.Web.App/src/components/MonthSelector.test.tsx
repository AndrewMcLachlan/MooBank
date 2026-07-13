import { describe, it, expect, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MonthSelector } from "components/MonthSelector";

describe("MonthSelector", () => {
    it("checks every month when value defaults to all months selected", () => {
        render(<MonthSelector />);
        expect(screen.getByRole("checkbox", { name: "January" })).toBeChecked();
        expect(screen.getByRole("checkbox", { name: "December" })).toBeChecked();
    });

    it("checks no months when value is 0", () => {
        render(<MonthSelector value={0} />);
        expect(screen.getByRole("checkbox", { name: "January" })).not.toBeChecked();
        expect(screen.getByRole("checkbox", { name: "December" })).not.toBeChecked();
    });

    it("checks only the months set in the bitmask", () => {
        // January (bit 0) and March (bit 2) selected: 0b101 = 5
        render(<MonthSelector value={5} />);
        expect(screen.getByRole("checkbox", { name: "January" })).toBeChecked();
        expect(screen.getByRole("checkbox", { name: "February" })).not.toBeChecked();
        expect(screen.getByRole("checkbox", { name: "March" })).toBeChecked();
    });

    it("toggles the bit for the clicked month on", async () => {
        const user = userEvent.setup();
        const onChange = vi.fn();
        render(<MonthSelector value={0} onChange={onChange} />);
        await user.click(screen.getByRole("checkbox", { name: "January" }));
        expect(onChange).toHaveBeenCalledWith(1);
    });

    it("toggles the bit for the clicked month off", async () => {
        const user = userEvent.setup();
        const onChange = vi.fn();
        render(<MonthSelector value={1} onChange={onChange} />);
        await user.click(screen.getByRole("checkbox", { name: "January" }));
        expect(onChange).toHaveBeenCalledWith(0);
    });

    it("selects all months when the All preset is clicked", async () => {
        const user = userEvent.setup();
        const onChange = vi.fn();
        render(<MonthSelector value={0} onChange={onChange} />);
        await user.click(screen.getByRole("button", { name: "All" }));
        expect(onChange).toHaveBeenCalledWith(4095);
    });

    it("clears all months when the clear control is clicked", async () => {
        const user = userEvent.setup();
        const onChange = vi.fn();
        render(<MonthSelector value={4095} onChange={onChange} />);
        await user.click(screen.getByRole("button", { name: "Clear all months" }));
        expect(onChange).toHaveBeenCalledWith(0);
    });
});
