import type { Tag } from "api/types.gen";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { updateTagMutation, getTagsQueryKey } from "api/@tanstack/react-query.gen";
import { UpdateTag } from "api/types.gen";

export const useUpdateTag = () => {

    const queryClient = useQueryClient();

    const { mutate, ...rest } = useMutation({
        ...updateTagMutation(),
        onSuccess: (data) => {
            queryClient.setQueryData<Tag>(getTagsQueryKey(), data);
            const allTags = queryClient.getQueryData<Tag[]>(getTagsQueryKey());
            if (!allTags) return;

            const tagIndex = allTags.findIndex(r => r.id === data.id);

            allTags.splice(tagIndex, 1, data);

            const newTags = allTags.sort((t1, t2) => t1.name.localeCompare(t2.name));
            queryClient.setQueryData<Tag[]>(getTagsQueryKey(), newTags);
            queryClient.invalidateQueries({ queryKey: getTagsQueryKey() });
        }
    });

    return {
        mutate: (variables: Tag) => {
            mutate({
                body: {
                    name: variables.name?.trim(),
                    colour: variables.colour as UpdateTag["colour"],
                    excludeFromReporting: variables.settings?.excludeFromReporting ?? false,
                    applySmoothing: variables.settings?.applySmoothing ?? false,
                },
                path: { id: variables.id },
            });
        },
        ...rest,
    };
}
