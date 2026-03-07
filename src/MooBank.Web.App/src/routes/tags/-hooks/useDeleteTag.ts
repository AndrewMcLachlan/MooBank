import type { Tag } from "api/types.gen";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { deleteTagMutation, getTagsQueryKey } from "api/@tanstack/react-query.gen";

export const useDeleteTag = () => {

    const queryClient = useQueryClient();

    return useMutation({
        ...deleteTagMutation(),
        onSuccess: (_data, variables) => {
            let allTags = queryClient.getQueryData<Tag[]>(getTagsQueryKey());
            if (!allTags) return;
            allTags = allTags.filter(r => r.id !== variables.path!.id);
            allTags = allTags.sort((t1, t2) => t1.name.localeCompare(t2.name));
            queryClient.setQueryData<Tag[]>(getTagsQueryKey(), allTags);
        }
    });
}
