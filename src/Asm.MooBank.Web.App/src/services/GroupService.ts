import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { getAllGroupsOptions, getAllGroupsQueryKey, getGroupOptions, createGroupMutation, updateGroupMutation } from "api/@tanstack/react-query.gen";
import { Group } from "../models";

export const useGroups = () => useQuery({ ...getAllGroupsOptions() });

export const useGroup = (id: string) => useQuery({ ...getGroupOptions({ path: { id } }) });

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
            mutateAsync({ body: group, path: { id: group.id } });
        }, ...rest,
    };
};
