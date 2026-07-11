import type { Tag } from "api/types.gen";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { updateTagMutation, getTagsQueryKey } from "api/@tanstack/react-query.gen";
import type { UpdateTag } from "api/types.gen";
import { toast } from "@andrewmclachlan/moo-ds";

export const useUpdateTag = () => {

    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...updateTagMutation(),
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: getTagsQueryKey() });
        }
    });

    return {
        ...rest,
        mutate: (variables: Tag) => {
            toast.promise(mutateAsync({
                body: {
                    name: variables.name?.trim(),
                    colour: variables.colour as UpdateTag["colour"],
                    excludeFromReporting: variables.settings?.excludeFromReporting ?? false,
                    applySmoothing: variables.settings?.applySmoothing ?? false,
                    budgetCategory: variables.settings?.budgetCategory ?? false,
                },
                path: { id: variables.id },
            }), { pending: "Updating tag", success: "Tag updated", error: "Failed to update tag" });
        },
    };
}
