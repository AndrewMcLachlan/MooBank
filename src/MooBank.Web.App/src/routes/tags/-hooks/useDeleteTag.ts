import { useMutation, useQueryClient } from "@tanstack/react-query";
import { deleteTagMutation, getTagsQueryKey } from "api/@tanstack/react-query.gen";
import { toast } from "@andrewmclachlan/moo-ds";

export const useDeleteTag = () => {

    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...deleteTagMutation(),
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: getTagsQueryKey() });
        }
    });

    return {
        ...rest,
        mutate: (variables: { path: { id: number } }) =>
            toast.promise(mutateAsync(variables as any), { pending: "Deleting tag", success: "Tag deleted", error: "Failed to delete tag" }),
    };
}
