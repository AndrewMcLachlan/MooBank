import type { Tag } from "api/types.gen";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createTagByNameMutation, getTagsQueryKey } from "api/@tanstack/react-query.gen";

export const useCreateTag = () => {

    const queryClient = useQueryClient();

    const { mutate, mutateAsync, ...rest } = useMutation({
        ...createTagByNameMutation(),
        onSuccess: (data) => {
            const allTags = queryClient.getQueryData<Tag[]>(getTagsQueryKey());
            if (!allTags) return;
            const newTags = [data, ...allTags].sort((t1, t2) => t1.name.localeCompare(t2.name));
            queryClient.setQueryData<Tag[]>(getTagsQueryKey(), newTags);
        }
    });

    const wrappedMutate = (variables: { name: string } | Tag) => {
        const name = (variables as Tag).name?.trim() ?? (variables as { name: string }).name.trim();
        const tags = (variables as Tag).tags?.map(t => t.id) ?? [];
        mutate({ body: { tags }, path: { name: encodeURIComponent(name) } } as any);
    };

    const wrappedMutateAsync = async (variables: { name: string } | Tag): Promise<Tag> => {
        const name = (variables as Tag).name?.trim() ?? (variables as { name: string }).name.trim();
        const tags = (variables as Tag).tags?.map(t => t.id) ?? [];
        const result = await mutateAsync({ body: { tags }, path: { name: encodeURIComponent(name) } } as any);
        return result as Tag;
    };

    return {
        mutate: wrappedMutate,
        mutateAsync: wrappedMutateAsync,
        ...rest,
    };
}
