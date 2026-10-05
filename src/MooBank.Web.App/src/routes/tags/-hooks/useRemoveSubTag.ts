import type { Tag } from "api/types.gen";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { removeSubTagMutation, getTagsQueryKey } from "api/@tanstack/react-query.gen";
import { toast } from "@andrewmclachlan/moo-ds";

export const useRemoveSubTag = () => {

    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...removeSubTagMutation(),
        onSuccess: (_data, variables) => {
            const { id, subTagId } = variables.path;
            const allTags = queryClient.getQueryData<Tag[]>(getTagsQueryKey());
            if (!allTags) return;

            queryClient.setQueryData<Tag[]>(getTagsQueryKey(), allTags.map(t =>
                t.id === id ? { ...t, tags: t.tags.filter(child => child.id !== subTagId) } : t));
        }
    });

    return {
        ...rest,
        mutate: (variables: { path: { id: number, subTagId: number } }) =>
            toast.promise(mutateAsync(variables as any), { pending: "Removing sub tag", success: "Sub tag removed", error: "Failed to remove sub tag" }),
    };
}
