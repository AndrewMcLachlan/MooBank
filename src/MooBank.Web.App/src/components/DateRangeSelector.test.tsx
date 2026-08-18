import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

import { DateRangePanel, DateRangeSelector } from "components/DateRangeSelector";
import { dateRangeStorageKey } from "hooks/dateRange";

vi.mock("@andrewmclachlan/moo-ds", () => ({
    // Render the overlay inline so the popover contents are assertable without opening it, and
    // hand the panel a close spy in place of the real one.
    OverlayTrigger: ({ children, overlay }: { children?: React.ReactNode; overlay?: (close: () => void) => React.ReactNode }) =>
        <>{children}{overlay?.(() => { })}</>,
    Popover: Object.assign(
        ({ children }: { children?: React.ReactNode }) => <div>{children}</div>,
        { Body: ({ children }: { children?: React.ReactNode }) => <div>{children}</div> },
    ),
}));

// Fixed "today" so presets resolve to known months: Last Month is July 2025.
const today = new Date(2025, 7, 15);

const setup = () => userEvent.setup();

beforeEach(() => {
    localStorage.clear();
    window.history.pushState({}, "", "?");
    // shouldAdvanceTime keeps timers ticking in real time: user-event's own waits run on them, and
    // freezing the clock outright deadlocks every interaction.
    vi.useFakeTimers({ shouldAdvanceTime: true });
    vi.setSystemTime(today);
});

afterEach(() => {
    vi.useRealTimers();
    localStorage.clear();
});

const panel = (props: Partial<React.ComponentProps<typeof DateRangePanel>> = {}) => {
    const onSelect = vi.fn();
    const onClose = vi.fn();
    render(<DateRangePanel selection={{ preset: "1" }} onSelect={onSelect} onClose={onClose} {...props} />);
    return { onSelect, onClose };
};

describe("DateRangePanel presets", () => {
    it("offers every ready-made period", () => {
        panel();

        expect(screen.getByRole("button", { name: "This Month" })).toBeInTheDocument();
        expect(screen.getByRole("button", { name: "Last 12 months" })).toBeInTheDocument();
        expect(screen.getByRole("button", { name: "All time" })).toBeInTheDocument();
    });

    it("marks the selected preset as current", () => {
        panel({ selection: { preset: "3" } });

        expect(screen.getByRole("button", { name: "Last 3 months" })).toHaveAttribute("aria-current", "true");
        expect(screen.getByRole("button", { name: "Last Month" })).not.toHaveAttribute("aria-current");
    });

    it("applies a preset on click and closes, with no Go button in the way", async () => {
        const user = setup();
        const { onSelect, onClose } = panel();

        await user.click(screen.getByRole("button", { name: "Last 6 months" }));

        expect(onSelect).toHaveBeenCalledExactlyOnceWith({ preset: "4" });
        expect(onClose).toHaveBeenCalledOnce();
    });

    it("marks the months a preset covers, so you can see which ones it resolved to", () => {
        panel({ selection: { preset: "3" } });   // Last 3 months — May, Jun, Jul 2025

        expect(screen.getByRole("button", { name: "April 2025" })).toHaveAttribute("aria-pressed", "false");
        expect(screen.getByRole("button", { name: "May 2025" })).toHaveAttribute("aria-pressed", "true");
        expect(screen.getByRole("button", { name: "June 2025" })).toHaveAttribute("aria-pressed", "true");
        expect(screen.getByRole("button", { name: "July 2025" })).toHaveAttribute("aria-pressed", "true");
        expect(screen.getByRole("button", { name: "August 2025" })).toHaveAttribute("aria-pressed", "false");
    });

    it("marks only the visible part of a preset that spans years", async () => {
        const user = setup();
        panel({ selection: { preset: "5" } });   // Last 12 months — Aug 2024 to Jul 2025

        expect(screen.getByRole("button", { name: "July 2025" })).toHaveAttribute("aria-pressed", "true");
        expect(screen.getByRole("button", { name: "August 2025" })).toHaveAttribute("aria-pressed", "false");

        await user.click(screen.getByRole("button", { name: "Show 2024" }));

        expect(screen.getByRole("button", { name: "July 2024" })).toHaveAttribute("aria-pressed", "false");
        expect(screen.getByRole("button", { name: "August 2024" })).toHaveAttribute("aria-pressed", "true");
    });
});

describe("DateRangePanel custom range", () => {
    it("does not apply anything on the first month click", async () => {
        const user = setup();
        const { onSelect, onClose } = panel();

        await user.click(screen.getByRole("button", { name: "March 2025" }));

        expect(onSelect).not.toHaveBeenCalled();
        expect(onClose).not.toHaveBeenCalled();
    });

    it("applies the range on the second click and closes", async () => {
        const user = setup();
        const { onSelect, onClose } = panel();

        await user.click(screen.getByRole("button", { name: "March 2025" }));
        await user.click(screen.getByRole("button", { name: "June 2025" }));

        expect(onSelect).toHaveBeenCalledExactlyOnceWith({ startMonth: "2025-03", endMonth: "2025-06" });
        expect(onClose).toHaveBeenCalledOnce();
    });

    it("orders the range when the second click is earlier than the first", async () => {
        const user = setup();
        const { onSelect } = panel();

        await user.click(screen.getByRole("button", { name: "June 2025" }));
        await user.click(screen.getByRole("button", { name: "March 2025" }));

        expect(onSelect).toHaveBeenCalledExactlyOnceWith({ startMonth: "2025-03", endMonth: "2025-06" });
    });

    it("selects a single month when the same month is clicked twice", async () => {
        const user = setup();
        const { onSelect } = panel();

        await user.click(screen.getByRole("button", { name: "March 2025" }));
        await user.click(screen.getByRole("button", { name: "March 2025" }));

        expect(onSelect).toHaveBeenCalledExactlyOnceWith({ startMonth: "2025-03", endMonth: "2025-03" });
    });

    it("keeps the pending start while paging to another year", async () => {
        const user = setup();
        const { onSelect } = panel();

        await user.click(screen.getByRole("button", { name: "Show 2024" }));
        await user.click(screen.getByRole("button", { name: "November 2024" }));
        await user.click(screen.getByRole("button", { name: "Show 2025" }));
        await user.click(screen.getByRole("button", { name: "June 2025" }));

        expect(onSelect).toHaveBeenCalledExactlyOnceWith({ startMonth: "2024-11", endMonth: "2025-06" });
    });

    it("marks the months of the stored custom range", () => {
        panel({ selection: { startMonth: "2025-03", endMonth: "2025-06" } });

        expect(screen.getByRole("button", { name: "March 2025" })).toHaveAttribute("aria-pressed", "true");
        expect(screen.getByRole("button", { name: "April 2025" })).toHaveAttribute("aria-pressed", "true");
        expect(screen.getByRole("button", { name: "June 2025" })).toHaveAttribute("aria-pressed", "true");
        expect(screen.getByRole("button", { name: "July 2025" })).toHaveAttribute("aria-pressed", "false");
    });

    it("previews the range under the pointer before it is applied", async () => {
        const user = setup();
        panel();

        await user.click(screen.getByRole("button", { name: "March 2025" }));
        await user.hover(screen.getByRole("button", { name: "May 2025" }));

        expect(screen.getByRole("button", { name: "April 2025" })).toHaveAttribute("aria-pressed", "true");
        expect(screen.getByRole("button", { name: "June 2025" })).toHaveAttribute("aria-pressed", "false");
    });
});

describe("DateRangePanel year", () => {
    it("opens on the year a preset resolves into", () => {
        panel({ selection: { preset: "6" } });   // Last year — 2024

        expect(screen.getByText("2024")).toBeInTheDocument();
    });

    it("opens on the year a custom range ends in", () => {
        panel({ selection: { startMonth: "2022-05", endMonth: "2023-01" } });

        expect(screen.getByText("2023")).toBeInTheDocument();
    });

    it("steps a year at a time", async () => {
        const user = setup();
        panel();

        await user.click(screen.getByRole("button", { name: "Show 2026" }));

        expect(screen.getByText("2026")).toBeInTheDocument();
        expect(screen.getByRole("button", { name: "March 2026" })).toBeInTheDocument();
    });
});

describe("DateRangePanel resolved range", () => {
    it("shows the days a preset resolves to", () => {
        panel({ selection: { preset: "1" } });

        expect(screen.getByText("1 Jul 2025 – 31 Jul 2025")).toBeInTheDocument();
    });

    it("shows the days a custom range resolves to, month ends included", () => {
        panel({ selection: { startMonth: "2025-03", endMonth: "2025-06" } });

        expect(screen.getByText("1 Mar 2025 – 30 Jun 2025")).toBeInTheDocument();
    });

    it("shows the pending range before it is applied", async () => {
        const user = setup();
        panel();

        await user.click(screen.getByRole("button", { name: "February 2025" }));

        expect(screen.getByText("1 Feb 2025 – 28 Feb 2025")).toBeInTheDocument();
    });
});

describe("DateRangePanel dismissal", () => {
    it("closes on Escape", async () => {
        const user = setup();
        const { onClose, onSelect } = panel();

        await user.keyboard("{Escape}");

        expect(onClose).toHaveBeenCalledOnce();
        expect(onSelect).not.toHaveBeenCalled();
    });
});

describe("DateRangeSelector", () => {
    it("labels the trigger with the stored preset", () => {
        localStorage.setItem(dateRangeStorageKey, JSON.stringify({ preset: "3" }));

        render(<DateRangeSelector />);

        expect(screen.getByRole("button", { name: /^Period: Last 3 months/ })).toBeInTheDocument();
    });

    it("labels the trigger with a compact month range for a custom selection", () => {
        localStorage.setItem(dateRangeStorageKey, JSON.stringify({ startMonth: "2025-03", endMonth: "2025-06" }));

        render(<DateRangeSelector />);

        expect(screen.getByRole("button", { name: /^Period: Mar – Jun 2025/ })).toBeInTheDocument();
    });

    it("hands the consumer the resolved period on mount", () => {
        localStorage.setItem(dateRangeStorageKey, JSON.stringify({ startMonth: "2025-03", endMonth: "2025-06" }));
        const onChange = vi.fn();

        render(<DateRangeSelector onChange={onChange} />);

        expect(onChange).toHaveBeenCalledExactlyOnceWith({
            startDate: new Date(2025, 2, 1),
            endDate: new Date(2025, 5, 30, 23, 59, 59, 999),
        });
    });

    it("hands the consumer a new period when a month range is chosen, and persists it", async () => {
        const user = setup();
        const onChange = vi.fn();

        render(<DateRangeSelector onChange={onChange} />);
        onChange.mockClear();

        await user.click(screen.getByRole("button", { name: "March 2025" }));
        await user.click(screen.getByRole("button", { name: "June 2025" }));

        expect(onChange).toHaveBeenCalledExactlyOnceWith({
            startDate: new Date(2025, 2, 1),
            endDate: new Date(2025, 5, 30, 23, 59, 59, 999),
        });
        expect(JSON.parse(localStorage.getItem(dateRangeStorageKey))).toEqual({ startMonth: "2025-03", endMonth: "2025-06" });
    });
});
