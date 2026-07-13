import { describe, it, expect } from "vitest";
import type { Rule } from "api/types.gen";
import { sortRules } from "utils/rules";

const makeRule = (id: number, contains: string): Rule => ({
    id,
    contains,
    tags: [],
});

describe("sortRules", () => {
    it("sorts ascending by contains, case-insensitively", () => {
        const rules = [makeRule(1, "zebra"), makeRule(2, "Apple"), makeRule(3, "mango")];
        const sorted = [...rules].sort(sortRules("Ascending"));
        expect(sorted.map(r => r.contains)).toEqual(["Apple", "mango", "zebra"]);
    });

    it("sorts descending by contains, case-insensitively", () => {
        const rules = [makeRule(1, "zebra"), makeRule(2, "Apple"), makeRule(3, "mango")];
        const sorted = [...rules].sort(sortRules("Descending"));
        expect(sorted.map(r => r.contains)).toEqual(["zebra", "mango", "Apple"]);
    });

    it("returns 0 for equal names regardless of case", () => {
        expect(sortRules("Ascending")(makeRule(1, "Woolworths"), makeRule(2, "WOOLWORTHS"))).toBe(0);
        expect(sortRules("Descending")(makeRule(1, "Woolworths"), makeRule(2, "woolworths"))).toBe(0);
    });
});
