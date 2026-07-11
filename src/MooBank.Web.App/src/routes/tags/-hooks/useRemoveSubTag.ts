import { useMutation, useQueryClient } from "@tanstack/react-query";
import { removeSubTagMutation, getTagsQueryKey } from "api/@tanstack/react-query.gen";
import { toast } from "@andrewmclachlan/moo-ds";

export const useRemoveSubTag = () => {

    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...removeSubTagMutation(),
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: getTagsQueryKey() });
        }
    });

    return {
        ...rest,
        mutate: (variables: { path: { id: number, subTagId: number } }) =>
            toast.promise(mutateAsync(variables as any), { pending: "Removing tag", success: "Tag removed", error: "Failed to remove tag" }),
    };
}
