import * as Models from "../models";
import { useApiGet, useApiDelete, useApiDatalessPut } from "./api";
import { useMutation, useQueryClient } from "react-query";
import { useHttpClient } from "../components";

interface TransactionTagVariables {
    name: string;
}

export const useTags = () => useApiGet<Models.TransactionTag[]>(["tags"], `api/transaction/tags`);

export const useCreateTag = () => {

    const queryClient = useQueryClient();
    const httpClient = useHttpClient();

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

    return useApiDelete<{ id: number }>((variables) => `api/transaction/tags/${variables.id}`, {
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

    return useApiDatalessPut<Models.TransactionTag, { tagId: number, subTagId: number }>((variables) => `api/transaction/tags/${variables.tagId}/tags/${variables.subTagId}`, {
        onSuccess: (data: Models.TransactionTag, variables) => {
            queryClient.setQueryData<Models.TransactionTag>(["tags", { id: variables.tagId }], data);
        }
    });
}

export const useRemoveSubTag = () => {

    const queryClient = useQueryClient();

    return useApiDelete<{ tagId: number, subTagId: number }>((variables) => `api/transaction/tags/${variables.tagId}/tags/${variables.subTagId}`, {
        onSuccess: (data: null, variables) => {
            const tag = queryClient.getQueryData<Models.TransactionTag>(["tags", { id: variables.tagId }]);
            tag.tags =tag.tags.filter(t => t.id !== variables.subTagId);
        }
    });
}
