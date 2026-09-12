import { describe, it, expect, beforeEach, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";

const mocks = vi.hoisted(() => ({
    clear: vi.fn(),
    setFilterTags: vi.fn(),
    setFilterType: vi.fn(),
    setFilterDescription: vi.fn(),
    setFilterTagged: vi.fn(),
    setFilterNetZero: vi.fn(),
    filter: {
        filterDescription: "",
        filterTagged: false,
        filterNetZero: false,
        filterTags: [1] as number[],
        filterType: "Expense",
    },
}));

vi.mock("../hooks/useFilterPanel", () => ({
    useFilterPanel: () => ({
        ...mocks.filter,
        period: { startDate: null, endDate: null },
        clear: mocks.clear,
        setFilterDescription: mocks.setFilterDescription,
        setFilterTagged: mocks.setFilterTagged,
        setFilterNetZero: mocks.setFilterNetZero,
        setFilterTags: mocks.setFilterTags,
        setFilterType: mocks.setFilterType,
        setPeriod: vi.fn(),
    }),
}));

vi.mock("hooks/useTags", () => ({
    useTags: () => ({ data: [{ id: 1, name: "Groceries" }] }),
}));

vi.mock("components/DateRangeSelector", () => ({
    DateRangeSelector: () => <div data-testid="period">Jan – Jun 2026</div>,
}));

vi.mock("components", async (importOriginal) => ({
    ...(await importOriginal<typeof import("components")>()),
    TagSelector: () => <div data-testid="tag-selector" />,
}));

import { TransactionsFilterBar } from "./TransactionsFilterBar";

beforeEach(() => {
    vi.clearAllMocks();
    mocks.filter = {
        filterDescription: "",
        filterTagged: false,
        filterNetZero: false,
        filterTags: [1],
        filterType: "Expense",
    };
});

describe("TransactionsFilterBar", () => {

    it("keeps the period on the bar, since it scopes the figures above", () => {
        render(<TransactionsFilterBar />);
        expect(screen.getByTestId("period")).toBeInTheDocument();
    });

    it("counts the active filters, excluding the period", () => {
        render(<TransactionsFilterBar />);
        expect(screen.getByText("2")).toBeInTheDocument();
    });

    // Scoped to the chips: the sheet's Type select carries the same words.
    it("names each active filter as a chip", () => {
        const { container } = render(<TransactionsFilterBar />);
        const chips = [...container.querySelectorAll(".filter-chip-label")].map(c => c.textContent);
        expect(chips).toEqual(["Groceries", "Expense"]);
    });

    it("clears only that filter when a chip is dismissed", async () => {
        const user = userEvent.setup();
        render(<TransactionsFilterBar />);
        await user.click(screen.getByRole("button", { name: "Remove Expense" }));
        expect(mocks.setFilterType).toHaveBeenCalledWith("");
        expect(mocks.setFilterTags).not.toHaveBeenCalled();
    });

    it("clears everything from the bar's own control", async () => {
        const user = userEvent.setup();
        render(<TransactionsFilterBar />);
        await user.click(screen.getByRole("button", { name: "Clear" }));
        expect(mocks.clear).toHaveBeenCalledTimes(1);
    });

    it("offers no Clear when nothing is active", () => {
        mocks.filter = { filterDescription: "", filterTagged: false, filterNetZero: false, filterTags: [], filterType: "" };
        render(<TransactionsFilterBar />);
        expect(screen.queryByRole("button", { name: "Clear" })).not.toBeInTheDocument();
    });

    it("opens the sheet from the Filters button", async () => {
        const user = userEvent.setup();
        const { baseElement } = render(<TransactionsFilterBar />);
        expect(baseElement.querySelector(".offcanvas-bottom.show")).not.toBeInTheDocument();
        await user.click(screen.getByRole("button", { name: /filters/i }));
        expect(baseElement.querySelector(".offcanvas-bottom.show")).toBeInTheDocument();
    });
});
