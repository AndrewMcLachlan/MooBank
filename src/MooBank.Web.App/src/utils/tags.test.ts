import { describe, it, expect } from "vitest";
import type { Tag } from "api/types.gen";
import { sortTags } from "utils/tags";

const makeTag = (id: number, name: string): Tag => ({
    id,
    name,
    tags: [],
    settings: { applySmoothing: false, excludeFromReporting: false, budgetCategory: false },
});

describe("sortTags", () => {
    it("sorts ascending by name, case-insensitively", () => {
        const tags = [makeTag(1, "zebra"), makeTag(2, "Apple"), makeTag(3, "mango")];
        const sorted = [...tags].sort(sortTags("Ascending"));
        expect(sorted.map(t => t.name)).toEqual(["Apple", "mango", "zebra"]);
    });

    it("sorts descending by name, case-insensitively", () => {
        const tags = [makeTag(1, "zebra"), makeTag(2, "Apple"), makeTag(3, "mango")];
        const sorted = [...tags].sort(sortTags("Descending"));
        expect(sorted.map(t => t.name)).toEqual(["zebra", "mango", "Apple"]);
    });

    it("returns 0 for equal names regardless of case", () => {
        expect(sortTags("Ascending")(makeTag(1, "Groceries"), makeTag(2, "GROCERIES"))).toBe(0);
        expect(sortTags("Descending")(makeTag(1, "Groceries"), makeTag(2, "groceries"))).toBe(0);
    });
});
