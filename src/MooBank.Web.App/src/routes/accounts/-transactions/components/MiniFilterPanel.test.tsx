import { describe, it, expect, beforeEach, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { LinkProvider } from "@andrewmclachlan/moo-ds";

const mocks = vi.hoisted(() => ({
    setFilterType: vi.fn(),
}));

vi.mock("../hooks/useFilterPanel", () => ({
    useFilterPanel: () => ({
        filterDescription: "",
        filterTagged: false,
        filterNetZero: false,
        filterTags: [],
        filterType: "",
        setFilterDescription: vi.fn(),
        setFilterTagged: vi.fn(),
        setFilterNetZero: vi.fn(),
        setFilterTags: vi.fn(),
        setFilterType: mocks.setFilterType,
        setPeriod: vi.fn(),
    }),
}));

vi.mock("components/DateRangeSelector", () => ({
    DateRangeSelector: () => <div data-testid="period" />,
}));

vi.mock("components", async (importOriginal) => ({
    ...(await importOriginal<typeof import("components")>()),
    TagSelector: () => <div data-testid="tag-selector" />,
}));

import { MiniFilterPanel } from "./MiniFilterPanel";

const renderPanel = () =>
    render(
        <LinkProvider
            LinkComponent={({ to, children, ...rest }: any) => <a href={to} {...rest}>{children}</a>}
            NavLinkComponent={({ to, children, ...rest }: any) => <a href={to} {...rest}>{children}</a>}
        >
            <MiniFilterPanel />
        </LinkProvider>,
    );

beforeEach(() => {
    vi.clearAllMocks();
});

describe("MiniFilterPanel", () => {

    it.each([["All", ""], ["Income", "Credit"], ["Expense", "Debit"]])("filters %s as '%s'", async (label, type) => {
        const user = userEvent.setup();
        renderPanel();
        const select = screen.getByRole("combobox", { name: "Filter by income or expense" });
        if (label === "All") await user.selectOptions(select, "Income");
        await user.selectOptions(select, label);
        expect(mocks.setFilterType).toHaveBeenLastCalledWith(type);
    });
});
