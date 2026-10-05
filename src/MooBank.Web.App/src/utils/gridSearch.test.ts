import { describe, expect, it } from "vitest";

import { clampPage, isSearchChange, parsePage, parsePageSize, validateGridSearch, validatePageSearch } from "./gridSearch";

describe("parsePage", () => {
    it.each([
        ["2", 2],
        [3, 3],
        ["1", undefined],
        ["0", undefined],
        ["-4", undefined],
        ["2.5", undefined],
        ["abc", undefined],
        [undefined, undefined],
    ])("parses %j as %j", (value, expected) => {
        expect(parsePage(value)).toBe(expected);
    });
});

describe("parsePageSize", () => {
    it.each([
        ["50", 50],
        [10, 10],
        ["37", undefined],
        ["0", undefined],
        [undefined, undefined],
    ])("parses %j as %j", (value, expected) => {
        expect(parsePageSize(value)).toBe(expected);
    });
});

describe("validateGridSearch", () => {
    const defaults = { sortField: "name", sortDirection: "Ascending" as const, sortFields: ["name", "type"] };

    it("returns an empty search for an empty query string", () => {
        expect(validateGridSearch({}, defaults)).toEqual({});
    });

    it("keeps valid non-default values", () => {
        expect(validateGridSearch({ page: "3", pageSize: "100", search: "bank", sortField: "type", sortDirection: "Descending" }, defaults))
            .toEqual({ page: 3, pageSize: 100, search: "bank", sortField: "type", sortDirection: "Descending" });
    });

    it("drops values equal to the defaults", () => {
        expect(validateGridSearch({ page: "1", sortField: "name", sortDirection: "Ascending", search: "" }, defaults)).toEqual({});
    });

    it("drops invalid values", () => {
        expect(validateGridSearch({ page: "x", pageSize: "7", sortField: "nope", sortDirection: "Sideways", search: 4 }, defaults)).toEqual({});
    });

    it("accepts any sort field when the grid does not list them", () => {
        expect(validateGridSearch({ sortField: "anything" })).toEqual({ sortField: "anything" });
    });
});

describe("validatePageSearch", () => {
    it("keeps only the page", () => {
        expect(validatePageSearch({ page: "4", pageSize: "50", search: "x" })).toEqual({ page: 4 });
        expect(validatePageSearch({ page: "1" })).toEqual({});
    });
});

describe("isSearchChange", () => {
    it("treats empty values as equivalent", () => {
        expect(isSearchChange({}, { search: undefined, tagged: false, tags: [], description: "" })).toBe(false);
        expect(isSearchChange({ search: "a" }, { search: "a" })).toBe(false);
    });

    it("compares arrays by value", () => {
        expect(isSearchChange({ tags: [1, 2] }, { tags: [1, 2] })).toBe(false);
        expect(isSearchChange({ tags: [1, 2] }, { tags: [2] })).toBe(true);
    });

    it("detects a changed or newly set value", () => {
        expect(isSearchChange({ search: "a" }, { search: "ab" })).toBe(true);
        expect(isSearchChange({ search: "a" }, { search: undefined })).toBe(true);
        expect(isSearchChange({}, { tagged: true })).toBe(true);
    });

    it("ignores keys that are not being set", () => {
        expect(isSearchChange({ page: 3, search: "a" }, { search: "a" })).toBe(false);
    });
});

describe("clampPage", () => {
    it.each([
        [1, 5, 1],
        [5, 5, 5],
        [9, 5, 5],
        [3, 0, 1],
    ])("clamps page %i of %i pages to %i", (page, numberOfPages, expected) => {
        expect(clampPage(page, numberOfPages)).toBe(expected);
    });
});
