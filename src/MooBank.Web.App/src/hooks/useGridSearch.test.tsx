import { act, renderHook } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

import { useGridPageSize, useGridSearch, usePageInRange, useSearchTerm } from "./useGridSearch";

let currentSearch: Record<string, unknown> = {};
const navigate = vi.fn();

vi.mock("@tanstack/react-router", () => ({
    useSearch: () => currentSearch,
    useNavigate: () => navigate,
}));

const lastNavigation = () => {
    const { search, replace } = navigate.mock.lastCall[0];
    return { search: search(currentSearch), replace };
};

beforeEach(() => {
    currentSearch = {};
    navigate.mockReset();
    localStorage.clear();
});

describe("useGridSearch", () => {
    it("defaults to the first page and the given sort", () => {
        const { result } = renderHook(() => useGridSearch({ sortField: "name", sortDirection: "Descending" }));

        expect(result.current.page).toBe(1);
        expect(result.current.sortField).toBe("name");
        expect(result.current.sortDirection).toBe("Descending");
    });

    it("pushes page changes and omits the first page", () => {
        currentSearch = { page: 3, search: "x" };
        const { result } = renderHook(() => useGridSearch());

        act(() => result.current.setPage(4));
        expect(lastNavigation()).toEqual({ search: { page: 4, search: "x" }, replace: false });

        act(() => result.current.setPage(1));
        expect(lastNavigation().search.page).toBeUndefined();
    });

    it("omits default sort values", () => {
        const { result } = renderHook(() => useGridSearch({ sortField: "name", sortDirection: "Ascending" }));

        act(() => result.current.setSort("name", "Descending"));
        expect(lastNavigation().search).toEqual({ sortField: undefined, sortDirection: "Descending" });
    });

    it("replaces the entry and returns to the first page when the filter changes", () => {
        currentSearch = { page: 3, search: "a" };
        const { result } = renderHook(() => useGridSearch());

        act(() => result.current.setFilter({ search: "ab" }));
        expect(lastNavigation()).toEqual({ search: { page: undefined, search: "ab" }, replace: true });
    });

    it("keeps the page when the filter is unchanged", () => {
        currentSearch = { page: 3, search: "a" };
        const { result } = renderHook(() => useGridSearch());

        act(() => result.current.setFilter({ search: "a" }));
        expect(navigate).not.toHaveBeenCalled();
    });
});

describe("useGridPageSize", () => {
    it("falls back to the default without a stored preference", () => {
        const { result } = renderHook(() => useGridPageSize("grid-page-size", 20));
        expect(result.current[0]).toBe(20);
    });

    it("uses the stored preference when the URL has none", () => {
        localStorage.setItem("grid-page-size", "50");
        const { result } = renderHook(() => useGridPageSize("grid-page-size", 20));
        expect(result.current[0]).toBe(50);
    });

    it("prefers the URL over the stored preference", () => {
        localStorage.setItem("grid-page-size", "50");
        currentSearch = { pageSize: 100 };
        const { result } = renderHook(() => useGridPageSize("grid-page-size", 20));
        expect(result.current[0]).toBe(100);
    });

    it("stores the chosen size and omits the default from the URL", () => {
        const { result } = renderHook(() => useGridPageSize("grid-page-size", 20));

        act(() => result.current[1](50));
        expect(JSON.parse(localStorage.getItem("grid-page-size"))).toBe(50);
        expect(lastNavigation().search).toEqual({ pageSize: 50 });

        act(() => result.current[1](20));
        expect(lastNavigation().search).toEqual({ pageSize: undefined });
    });
});

describe("usePageInRange", () => {
    it("waits until the data is ready", () => {
        renderHook(() => usePageInRange(5, 1, false));
        expect(navigate).not.toHaveBeenCalled();
    });

    it("leaves a page in range alone", () => {
        renderHook(() => usePageInRange(2, 3, true));
        expect(navigate).not.toHaveBeenCalled();
    });

    it("moves a page beyond the end onto the last page", () => {
        renderHook(() => usePageInRange(9, 3, true));
        expect(lastNavigation()).toEqual({ search: { page: 3 }, replace: true });
    });
});

describe("useSearchTerm", () => {
    beforeEach(() => vi.useFakeTimers());
    afterEach(() => vi.useRealTimers());

    it("updates immediately and commits once typing pauses", () => {
        const onCommit = vi.fn();
        const { result } = renderHook(() => useSearchTerm("", onCommit));

        act(() => result.current[1]("a"));
        act(() => result.current[1]("ab"));
        expect(result.current[0]).toBe("ab");
        expect(onCommit).not.toHaveBeenCalled();

        act(() => vi.advanceTimersByTime(250));
        expect(onCommit).toHaveBeenCalledTimes(1);
        expect(onCommit).toHaveBeenCalledWith("ab");
    });

    it("keeps typing that is ahead of its own committed value", () => {
        const { result, rerender } = renderHook(({ value }) => useSearchTerm(value, () => {}), { initialProps: { value: "" } });

        act(() => result.current[1]("ab"));
        act(() => vi.advanceTimersByTime(250));
        act(() => result.current[1]("abc"));
        rerender({ value: "ab" });

        expect(result.current[0]).toBe("abc");
    });

    it("follows a value changed from elsewhere", () => {
        const { result, rerender } = renderHook(({ value }) => useSearchTerm(value, () => {}), { initialProps: { value: "groceries" } });

        rerender({ value: "fuel" });
        expect(result.current[0]).toBe("fuel");
    });
});
