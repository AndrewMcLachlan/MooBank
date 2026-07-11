import type { Tag } from "api/types.gen";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { removeSubTagMutation, getTagsQueryKey } from "api/@tanstack/react-query.gen";
import { toast } from "@andrewmclachlan/moo-ds";

export const useRemoveSubTag = () => {

    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...removeSubTagMutation(),
        onSuccess: (data) => {
            const tag = data as unknown as Tag;
            const allTags = queryClient.getQueryData<Tag[]>(getTagsQueryKey());
            if (!allTags) return;

            const tagIndex = allTags.findIndex(r => r.id === tag.id);
            if (tagIndex === -1) return;

            const newTags = [...allTags];
            newTags.splice(tagIndex, 1, tag);
            newTags.sort((t1, t2) => t1.name.localeCompare(t2.name));
            queryClient.setQueryData<Tag[]>(getTagsQueryKey(), newTags);
        }
    });

    return {
        ...rest,
        mutate: (variables: { path: { id: number, subTagId: number } }) =>
            toast.promise(mutateAsync(variables as any), { pending: "Removing sub tag", success: "Sub tag removed", error: "Failed to remove sub tag" }),
    };
}
