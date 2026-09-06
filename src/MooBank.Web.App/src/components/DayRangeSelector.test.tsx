import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

import { DayRangePanel, DayRangeSelector } from "components/DayRangeSelector";
import type { Period } from "models/dateFns";

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

// Fixed "today" so the presets resolve to known dates: Last Month is July 2025.
const today = new Date(2025, 7, 15);

const august = (from: number, to: number): Period => ({
    startDate: new Date(2025, 7, from),
    endDate: new Date(2025, 7, to),
});

const setup = () => userEvent.setup();

beforeEach(() => {
    // shouldAdvanceTime keeps timers ticking in real time: user-event's own waits run on them, and
    // freezing the clock outright deadlocks every interaction.
    vi.useFakeTimers({ shouldAdvanceTime: true });
    vi.setSystemTime(today);
});

afterEach(() => vi.useRealTimers());

const panel = (props: Partial<React.ComponentProps<typeof DayRangePanel>> = {}) => {
    const onSelect = vi.fn();
    const onClose = vi.fn();
    render(<DayRangePanel value={august(10, 20)} onSelect={onSelect} onClose={onClose} {...props} />);
    return { onSelect, onClose };
};

const day = (name: string) => screen.getByRole("button", { name });

describe("DayRangePanel", () => {
    /**
     * Given a range
     * When the panel opens
     * Then it shows the month that range ends in.
     */
    it("opens on the month the range ends in", () => {
        panel({ value: { startDate: new Date(2025, 6, 28), endDate: new Date(2025, 7, 3) } });

        expect(screen.getByText("August 2025")).toBeInTheDocument();
    });

    /**
     * Given the panel is open
     * When a single day is clicked
     * Then nothing is selected yet.
     */
    /* A half-made range must not reach the consumer: it would query for dates nobody asked for. */
    it("does not commit on the first click", async () => {
        const user = setup();
        const { onSelect, onClose } = panel();

        await user.click(day("12 August 2025"));

        expect(onSelect).not.toHaveBeenCalled();
        expect(onClose).not.toHaveBeenCalled();
    });

    /**
     * Given a start day has been clicked
     * When a second day is clicked
     * Then the range between them is selected and the panel closes.
     */
    it("selects the range on the second click", async () => {
        const user = setup();
        const { onSelect, onClose } = panel();

        await user.click(day("12 August 2025"));
        await user.click(day("19 August 2025"));

        expect(onSelect).toHaveBeenCalledWith({ startDate: new Date(2025, 7, 12), endDate: new Date(2025, 7, 19) });
        expect(onClose).toHaveBeenCalled();
    });

    /**
     * Given a start day has been clicked
     * When an earlier day is clicked second
     * Then the range is ordered rather than being empty.
     */
    it("orders a range chosen backwards", async () => {
        const user = setup();
        const { onSelect } = panel();

        await user.click(day("19 August 2025"));
        await user.click(day("12 August 2025"));

        expect(onSelect).toHaveBeenCalledWith({ startDate: new Date(2025, 7, 12), endDate: new Date(2025, 7, 19) });
    });

    /**
     * Given the current range
     * When the panel opens
     * Then its ends are marked so the selection is visible on the grid.
     */
    it("marks the ends of the current range", () => {
        panel({ value: august(10, 20) });

        expect(day("10 August 2025")).toHaveClass("range-start");
        expect(day("20 August 2025")).toHaveClass("range-end");
        expect(day("15 August 2025")).toHaveClass("in-range");
        expect(day("21 August 2025")).not.toHaveClass("in-range");
    });

    /**
     * Given a range spanning a month boundary
     * When the month is shown
     * Then the neighbouring days are still rendered, so the range reads as continuous.
     */
    it("shows the days either side of the month", () => {
        panel({ value: { startDate: new Date(2025, 6, 30), endDate: new Date(2025, 7, 2) } });

        expect(day("30 July 2025")).toHaveClass("outside");
        expect(day("30 July 2025")).toHaveClass("in-range");
    });

    /**
     * Given the panel is open
     * When the month is stepped
     * Then a different month is shown.
     */
    it("steps between months", async () => {
        const user = setup();
        panel({ value: august(10, 20) });

        await user.click(screen.getByRole("button", { name: "Show July 2025" }));

        expect(screen.getByText("July 2025")).toBeInTheDocument();
    });
});

describe("DayRangePanel presets", () => {
    it("offers the ready-made periods by default", () => {
        panel();

        expect(screen.getByRole("button", { name: "Last Month" })).toBeInTheDocument();
    });

    /* A bill is billed for the dates printed on it, never for "the last 3 months". */
    it("omits them when turned off", () => {
        panel({ presets: [] });

        expect(screen.queryByRole("button", { name: "Last Month" })).not.toBeInTheDocument();
    });

    /**
     * Given a range that matches a ready-made period
     * When the panel opens
     * Then that period reads as current, however the range was arrived at.
     */
    it("marks a preset current when the dates match it", () => {
        panel({ value: { startDate: new Date(2025, 6, 1), endDate: new Date(2025, 6, 31) } });

        expect(screen.getByRole("button", { name: "Last Month" })).toHaveAttribute("aria-current", "true");
    });

    it("selects a preset and closes", async () => {
        const user = setup();
        const { onSelect, onClose } = panel();

        await user.click(screen.getByRole("button", { name: "Last Month" }));

        expect(onSelect).toHaveBeenCalledWith({ startDate: new Date(2025, 6, 1), endDate: new Date(2025, 6, 31, 23, 59, 59, 999) });
        expect(onClose).toHaveBeenCalled();
    });
});

describe("DayRangePanel named periods", () => {

    // A period whose dates only the data can answer -- the consumer resolves it elsewhere.
    const named = [{ value: "Last", label: "Last period" }];

    /**
     * Given a preset that carries no dates
     * When it is chosen
     * Then its name is reported rather than a range, for the consumer to resolve.
     */
    it("reports the name of a period it cannot date", async () => {
        const user = setup();
        const { onSelect } = panel({ presets: named });

        await user.click(screen.getByRole("button", { name: "Last period" }));

        expect(onSelect).toHaveBeenCalledWith({ preset: "Last" });
    });

    /**
     * Given a named period is in force
     * When the panel opens
     * Then it reads as current, and nothing is marked on the calendar.
     */
    /* Marking a range would be a claim about which days were billed, which is not known here. */
    it("marks it current and leaves the calendar unmarked", () => {
        panel({ value: { preset: "Last" }, presets: named });

        expect(screen.getByRole("button", { name: "Last period" })).toHaveAttribute("aria-current", "true");
        expect(document.querySelectorAll(".day-range-days .in-range")).toHaveLength(0);
    });
});

describe("DayRangeSelector", () => {

    /* A named period says what it is; there are no dates to show. */
    it("labels the trigger with the name of a period it cannot date", () => {
        render(<DayRangeSelector value={{ preset: "Last" }} presets={[{ value: "Last", label: "Last period" }]} onChange={vi.fn()} />);

        expect(screen.getByRole("button", { name: /Dates: Last period/ })).toBeInTheDocument();
    });

    /* The label drops the year from the start when both ends share one, which is the common case. */
    it("labels the trigger with the range", () => {
        render(<DayRangeSelector value={august(10, 20)} onChange={vi.fn()} />);

        expect(screen.getByRole("button", { name: /Dates: 10 Aug - 20 Aug 2025/ })).toBeInTheDocument();
    });

    it("keeps the year on both ends when the range crosses one", () => {
        render(<DayRangeSelector value={{ startDate: new Date(2025, 11, 20), endDate: new Date(2026, 0, 10) }} onChange={vi.fn()} />);

        expect(screen.getByRole("button", { name: /Dates: 20 Dec 2025 - 10 Jan 2026/ })).toBeInTheDocument();
    });
});
