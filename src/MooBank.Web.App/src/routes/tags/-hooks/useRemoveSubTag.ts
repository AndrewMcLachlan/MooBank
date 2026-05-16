import type { Tag } from "api/types.gen";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { removeSubTagMutation, getTagsQueryKey } from "api/@tanstack/react-query.gen";

export const useRemoveSubTag = () => {

    const queryClient = useQueryClient();

    return useMutation({
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
}
