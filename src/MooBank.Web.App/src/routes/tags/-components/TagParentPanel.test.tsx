import { describe, it, expect, beforeEach, vi } from "vitest";
import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import type { Tag } from "api/types.gen";
import type { TagPanelProps } from "components";

const mocks = vi.hoisted(() => ({
    tags: vi.fn(),
    addSubTag: vi.fn(),
    removeSubTag: vi.fn(),
    createTag: vi.fn(),
}));

vi.mock("hooks/useTags", () => ({ useTags: () => mocks.tags() }));
vi.mock("hooks/useCreateTag", () => ({ useCreateTag: () => ({ mutate: mocks.createTag }) }));
vi.mock("../-hooks/useAddSubTag", () => ({ useAddSubTag: () => ({ mutate: mocks.addSubTag }) }));
vi.mock("../-hooks/useRemoveSubTag", () => ({ useRemoveSubTag: () => ({ mutate: mocks.removeSubTag }) }));
vi.mock("components", () => ({
    TagPanel: ({ selectedItems, items, onAdd, onRemove, onCreate }: TagPanelProps) => (
        <div>
            <ul aria-label="selected">{selectedItems.map(t => <li key={t.id}><button onClick={() => onRemove(t)}>{t.name}</button></li>)}</ul>
            <ul aria-label="available">{items.map(t => <li key={t.id}><button onClick={() => onAdd(t)}>{t.name}</button></li>)}</ul>
            <button onClick={() => onCreate("Essentials")}>create</button>
        </div>
    ),
}));

import { TagParentPanel } from "./TagParentPanel";

const tag = (id: number, name: string, children: Tag[] = []): Tag => ({ id, name, tags: children } as Tag);

const fruit = tag(3, "Fruit");
const groceries = tag(2, "Groceries", [fruit]);
const food = tag(1, "Food", [groceries]);
const travel = tag(4, "Travel");

const renderPanel = () => {
    mocks.tags.mockReturnValue({ data: [food, groceries, fruit, travel] });
    render(<TagParentPanel tag={groceries} />);
    return userEvent.setup();
};

const names = (label: string) =>
    within(screen.getByRole("list", { name: label })).queryAllByRole("button").map(b => b.textContent);

beforeEach(() => {
    mocks.addSubTag.mockReset().mockResolvedValue(undefined);
    mocks.removeSubTag.mockReset().mockResolvedValue(undefined);
    mocks.createTag.mockReset();
});

describe("TagParentPanel", () => {

    it("shows the tags that list this tag as a sub-tag", () => {
        renderPanel();

        expect(names("selected")).toEqual(["Food"]);
    });

    it("offers neither the tag itself nor its sub-tags as parents", () => {
        renderPanel();

        expect(names("available")).toEqual(["Travel"]);
    });

    it("adds a parent by adding this tag as the parent's sub-tag", async () => {
        const user = renderPanel();

        await user.click(screen.getByRole("button", { name: "Travel" }));

        expect(mocks.addSubTag).toHaveBeenCalledWith({ path: { id: 4, subTagId: 2 } });
        expect(names("selected")).toEqual(["Food", "Travel"]);
    });

    it("removes a parent by removing this tag from the parent's sub-tags", async () => {
        const user = renderPanel();

        await user.click(screen.getByRole("button", { name: "Food" }));

        expect(mocks.removeSubTag).toHaveBeenCalledWith({ path: { id: 1, subTagId: 2 } });
        expect(names("selected")).toEqual([]);
    });

    it("puts the parent back when the server refuses to remove it", async () => {
        mocks.removeSubTag.mockRejectedValue(new Error("Refused"));
        const user = renderPanel();

        await user.click(screen.getByRole("button", { name: "Food" }));

        await screen.findByRole("button", { name: "Food" });
        expect(names("selected")).toEqual(["Food"]);
    });

    it("creates a new parent with this tag as its sub-tag", async () => {
        const user = renderPanel();

        await user.click(screen.getByRole("button", { name: "create" }));

        expect(mocks.createTag).toHaveBeenCalledWith(expect.objectContaining({ name: "Essentials", tags: [groceries] }));
    });
});
