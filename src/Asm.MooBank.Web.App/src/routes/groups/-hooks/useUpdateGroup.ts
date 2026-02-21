import { useMutation, useQueryClient } from "@tanstack/react-query";
import { updateGroupMutation, getAllGroupsQueryKey } from "api/@tanstack/react-query.gen";
import type { Group } from "api/types.gen";

export const useUpdateGroup = () => {
    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...updateGroupMutation(),
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: getAllGroupsQueryKey() });
        },
    });

    return {
        mutateAsync: (group: Group) => {
            mutateAsync({ body: group, path: { id: group.id } } as any);
        }, ...rest,
    };
};
