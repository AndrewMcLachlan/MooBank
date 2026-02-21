import type { Tag } from "api/types.gen";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { addSubTagMutation, getTagsQueryKey } from "api/@tanstack/react-query.gen";

export const useAddSubTag = () => {

    const queryClient = useQueryClient();

    return useMutation({
        ...addSubTagMutation(),
        onSuccess: (data) => {
            queryClient.setQueryData<Tag>(getTagsQueryKey(), data);
            const allTags = queryClient.getQueryData<Tag[]>(getTagsQueryKey());
            if (!allTags) return;

            const tagIndex = allTags.findIndex(r => r.id === data.id);

            allTags.splice(tagIndex, 1, data);

            const newTags = allTags.sort((t1, t2) => t1.name.localeCompare(t2.name));
            queryClient.setQueryData<Tag[]>(getTagsQueryKey(), newTags);
        }
    });
}
