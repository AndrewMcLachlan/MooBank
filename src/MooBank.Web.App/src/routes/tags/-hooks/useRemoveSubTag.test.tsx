import { describe, it, expect, vi } from "vitest";
import { renderHook, act } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import type { PropsWithChildren } from "react";
import type { Tag } from "api/types.gen";
import { getTagsQueryKey } from "api/@tanstack/react-query.gen";
import { useRemoveSubTag } from "./useRemoveSubTag";

const removeSubTag = vi.hoisted(() => vi.fn());

vi.mock("api/sdk.gen", () => ({ removeSubTag }));
vi.mock("@andrewmclachlan/moo-ds", () => ({ toast: { promise: (p: Promise<unknown>) => p } }));

const tag = (id: number, name: string, children: Tag[] = []): Tag => ({ id, name, tags: children } as Tag);

describe("useRemoveSubTag", () => {

    /**
     * The endpoint answers 204 with no body, so the cached parent has to be updated from the request.
     * The parents column is derived from the cache, so a stale entry keeps showing the removed parent.
     */
    it("removes the sub-tag from the parent's cached entry", async () => {
        const fruit = tag(2, "Fruit");
        const client = new QueryClient({ defaultOptions: { mutations: { retry: false } } });
        client.setQueryData(getTagsQueryKey(), [tag(1, "Food", [fruit]), fruit]);
        removeSubTag.mockResolvedValue({ data: undefined });

        const { result } = renderHook(() => useRemoveSubTag(), {
            wrapper: ({ children }: PropsWithChildren) => <QueryClientProvider client={client}>{children}</QueryClientProvider>,
        });

        await act(async () => { await result.current.mutate({ path: { id: 1, subTagId: 2 } }); });

        const cached = client.getQueryData<Tag[]>(getTagsQueryKey());
        expect(cached.find(t => t.id === 1).tags).toEqual([]);
        expect(cached.map(t => t.id)).toEqual([1, 2]);
    });
});
