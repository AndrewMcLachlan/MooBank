import type { Tag } from "api/types.gen";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { deleteTagMutation, getTagsQueryKey } from "api/@tanstack/react-query.gen";
import { toast } from "@andrewmclachlan/moo-ds";

export const useDeleteTag = () => {

    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...deleteTagMutation(),
        onSuccess: (_data, variables) => {
            let allTags = queryClient.getQueryData<Tag[]>(getTagsQueryKey());
            if (!allTags) return;
            allTags = allTags.filter(r => r.id !== variables.path!.id);
            allTags = allTags.sort((t1, t2) => t1.name.localeCompare(t2.name));
            queryClient.setQueryData<Tag[]>(getTagsQueryKey(), allTags);
        }
    });

    return {
        ...rest,
        mutate: (variables: { path: { id: number } }) =>
            toast.promise(mutateAsync(variables as any), { pending: "Deleting tag", success: "Tag deleted", error: "Failed to delete tag" }),
    };
}
