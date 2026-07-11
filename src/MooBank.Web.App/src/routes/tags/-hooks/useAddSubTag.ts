import type { Tag } from "api/types.gen";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { addSubTagMutation, getTagsQueryKey } from "api/@tanstack/react-query.gen";
import { toast } from "@andrewmclachlan/moo-ds";

export const useAddSubTag = () => {

    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...addSubTagMutation(),
        onSuccess: (data) => {
            const allTags = queryClient.getQueryData<Tag[]>(getTagsQueryKey());
            if (!allTags) return;

            const tagIndex = allTags.findIndex(r => r.id === data.id);
            if (tagIndex === -1) return;

            const newTags = [...allTags];
            newTags.splice(tagIndex, 1, data);
            newTags.sort((t1, t2) => t1.name.localeCompare(t2.name));
            queryClient.setQueryData<Tag[]>(getTagsQueryKey(), newTags);
        }
    });

    return {
        ...rest,
        mutate: (variables: { path: { id: number, subTagId: number } }) =>
            toast.promise(mutateAsync(variables as any), { pending: "Adding sub tag", success: "Sub tag added", error: "Failed to add sub tag" }),
    };
}
