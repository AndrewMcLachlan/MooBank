import type { Tag } from "api/types.gen";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { updateTagMutation, getTagsQueryKey } from "api/@tanstack/react-query.gen";
import type { UpdateTag } from "api/types.gen";
import { toast } from "@andrewmclachlan/moo-ds";

export const useUpdateTag = () => {

    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...updateTagMutation(),
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
        mutate: (variables: Tag) =>
            toast.promise(mutateAsync({
                body: {
                    name: variables.name?.trim(),
                    colour: variables.colour as UpdateTag["colour"],
                    excludeFromReporting: variables.settings?.excludeFromReporting ?? false,
                    applySmoothing: variables.settings?.applySmoothing ?? false,
                    budgetCategory: variables.settings?.budgetCategory ?? false,
                },
                path: { id: variables.id },
            }), { pending: "Updating tag", success: "Tag updated", error: "Failed to update tag" }),
    };
}
