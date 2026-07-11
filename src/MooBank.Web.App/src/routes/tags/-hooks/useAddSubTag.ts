import { useMutation, useQueryClient } from "@tanstack/react-query";
import { addSubTagMutation, getTagsQueryKey } from "api/@tanstack/react-query.gen";
import { toast } from "@andrewmclachlan/moo-ds";

export const useAddSubTag = () => {

    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...addSubTagMutation(),
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: getTagsQueryKey() });
        }
    });

    return {
        ...rest,
        mutate: (variables: { path: { id: number, subTagId: number } }) =>
            toast.promise(mutateAsync(variables as any), { pending: "Adding tag", success: "Tag added", error: "Failed to add tag" }),
    };
}
