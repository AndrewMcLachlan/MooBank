import * as Models from "../models";
import { ServiceBase } from "./ServiceBase";
import HttpClient, { httpClient } from "./HttpClient";
import { useApiQuery } from "./useApiQuery";
import { useMutation, useQueryClient } from "react-query";

interface TransactionTagVariables {
    name: string;
}

export const useTags = () => useApiQuery<Models.TransactionTag[]>(["tags"], `api/transaction/tags`);

export const useCreateTag = () => {

    const queryClient = useQueryClient();

    return useMutation<Models.TransactionTag, null, TransactionTagVariables | Models.TransactionTag>(async (variables) => {

        const name = (variables as Models.TransactionTag).name || (variables.name).trim();
        const tags = (variables as Models.TransactionTag).tags || [];

        return (await httpClient.put<Models.TransactionTag>(`api/transaction/tags/${encodeURIComponent(name)}`, tags)).data;
    }, {
        onSuccess: (data: Models.TransactionTag) => {
            queryClient.setQueryData<Models.TransactionTag>(["tags", { id: data.id }], data);
            let allTags = queryClient.getQueryData<Models.TransactionTag[]>(["tags"]);
            allTags = allTags.sort((t1, t2) => t1.name.localeCompare(t2.name));
            queryClient.setQueryData<Models.TransactionTag[]>(["tags"], allTags);
        }
    });
}

export const useDeleteTag = () => {

    const queryClient = useQueryClient();

    return useMutation<null, null, { id: number }>(async (variables) => (await httpClient.delete(`api/transaction/tags/${variables.id}`)).data, {
        onSuccess: (variables: { id: number }) => {
            let allTags = queryClient.getQueryData<Models.TransactionTag[]>(["tags"]);
            allTags = allTags.filter(r => r.id !== (variables.id));
            allTags = allTags.sort((t1, t2) => t1.name.localeCompare(t2.name));
            queryClient.setQueryData<Models.TransactionTag[]>(["tags"], allTags);
        }
    });
}

export const useAddSubTag = () => {

    const queryClient = useQueryClient();

    return useMutation<Models.TransactionTag, null, { tagId: number, subTagId: number }>(async (variables) => (await httpClient.put<Models.TransactionTag>(`api/transaction/tags/${variables.tagId}/tags/${variables.subTagId}`)).data, {
        onSuccess: (data: Models.TransactionTag, variables: { tagId: number, subTagId: number }) => {
            queryClient.setQueryData<Models.TransactionTag>(["tags", { id: variables.tagId }], data);
        }
    });
}
export const useRemoveSubTag = () => {

    const queryClient = useQueryClient();

    return useMutation<Models.TransactionTag, null, { tagId: number, subTagId: number }>(async (variables) => (await httpClient.delete<Models.TransactionTag>(`api/transaction/tags/${variables.tagId}/tags/${variables.subTagId}`)).data, {
        onSuccess: (data: Models.TransactionTag, variables: { tagId: number, subTagId: number }) => {
            queryClient.setQueryData<Models.TransactionTag>(["tags", { id: variables.tagId }], data);
        }
    });
}
