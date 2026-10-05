import { describe, it, expect } from "vitest";
import type { Tag } from "api/types.gen";
import { getAncestorIds, getDescendantIds, getParents, getRelatableTags } from "./tagRelationships";

const tag = (id: number, name: string, children: number[] = []): Tag =>
    ({ id, name, tags: children.map(c => ({ id: c, name: `child ${c}`, tags: [] })) } as Tag);

const food = tag(1, "Food", [2, 3]);
const groceries = tag(2, "Groceries", [4]);
const takeaway = tag(3, "Takeaway");
const fruit = tag(4, "Fruit");
const household = tag(5, "Household", [2]);
const travel = tag(6, "Travel");

const allTags = [food, groceries, takeaway, fruit, household, travel];

describe("getParents", () => {

    it("finds every tag that lists the tag as a sub-tag", () => {
        expect(getParents(allTags, 2).map(t => t.name)).toEqual(["Food", "Household"]);
    });

    it("finds nothing for a top-level tag", () => {
        expect(getParents(allTags, 1)).toEqual([]);
    });
});

describe("getDescendantIds", () => {

    it("follows sub-tags down through every level", () => {
        expect([...getDescendantIds(allTags, 1)].sort()).toEqual([2, 3, 4]);
    });
});

describe("getAncestorIds", () => {

    it("follows parents up through every level and across multiple parents", () => {
        expect([...getAncestorIds(allTags, 4)].sort()).toEqual([1, 2, 5]);
    });

    it("terminates on a cyclic hierarchy", () => {
        const cyclic = [tag(1, "A", [2]), tag(2, "B", [1])];
        expect([...getAncestorIds(cyclic, 1)]).toEqual([2]);
    });
});

describe("getRelatableTags", () => {

    /**
     * The API rejects relating a tag to itself, to anything already above it or to anything already
     * below it, so offering those in a picker only produces an error toast.
     */
    it("offers only tags outside the tag's own ancestry and descent", () => {
        expect(getRelatableTags(allTags, 2).map(t => t.name)).toEqual(["Takeaway", "Travel"]);
    });
});
