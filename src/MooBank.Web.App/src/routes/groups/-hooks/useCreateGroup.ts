import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createGroupMutation, getAllGroupsQueryKey } from "api/@tanstack/react-query.gen";
import type { Group } from "api/types.gen";
import { toast } from "@andrewmclachlan/moo-ds";

export const useCreateGroup = () => {
    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...createGroupMutation(),
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: getAllGroupsQueryKey() });
        },
    });

    return {
        mutateAsync: (group: Group) =>
            toast.promise(mutateAsync({ body: { name: group.name, description: group.description ?? "", showTotal: group.showTotal, colour: group.colour } }), { pending: "Creating group", success: "Group created", error: "Failed to create group" }),
        ...rest,
    };
};
