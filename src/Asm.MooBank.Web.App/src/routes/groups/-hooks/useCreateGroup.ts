import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createGroupMutation, getAllGroupsQueryKey } from "api/@tanstack/react-query.gen";
import type { Group } from "api/types.gen";

export const useCreateGroup = () => {
    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...createGroupMutation(),
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: getAllGroupsQueryKey() });
        },
    });

    return {
        mutateAsync: (group: Group) => {
            mutateAsync({ body: { name: group.name, description: group.description ?? "", showTotal: group.showTotal } });
        }, ...rest,
    };
};
