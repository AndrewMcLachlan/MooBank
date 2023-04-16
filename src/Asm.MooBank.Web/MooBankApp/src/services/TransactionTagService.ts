import * as Models from "models";
import { useApiGet, useApiDelete, useApiDatalessPut, useHttpClient } from "@andrewmclachlan/mooapp";
import { useMutation, useQueryClient } from "react-query";

interface TransactionTagVariables {
    name: string;
}

export const useTags = () => useApiGet<Models.TransactionTag[]>(["tags"], "api/transaction/tags");

export const useTag = (id: number) => useApiGet<Models.TransactionTag>(["tags", id], `api/transaction/tags/${id}`);

export const useCreateTag = () => {

    const queryClient = useQueryClient();
    const httpClient = useHttpClient();

    return useMutation<Models.TransactionTag, null, TransactionTagVariables | Models.TransactionTag>(async (variables) => {

        const name = (variables as Models.TransactionTag).name?.trim() ?? (variables.name).trim();
        const tags = (variables as Models.TransactionTag).tags?.map(t => t.id) ?? [];

        return (await httpClient.put<Models.TransactionTag>(`api/transaction/tags/${encodeURIComponent(name)}`, tags)).data;
    }, {
        onSuccess: (data: Models.TransactionTag) => {
            queryClient.setQueryData<Models.TransactionTag>(["tags", { id: data.id }], data);
            const allTags = queryClient.getQueryData<Models.TransactionTag[]>(["tags"]);
            if (!allTags) return;
            const newTags = [data, ...allTags].sort((t1, t2) => t1.name.localeCompare(t2.name));
            queryClient.setQueryData<Models.TransactionTag[]>(["tags"], newTags);
        }
    });
}

export const useDeleteTag = () => {

    const queryClient = useQueryClient();

    return useApiDelete<{ id: number }>((variables) => `api/transaction/tags/${variables.id}`, {
        onSuccess: (_data, variables: { id: number }) => {
            let allTags = queryClient.getQueryData<Models.TransactionTag[]>(["tags"]);
            if (!allTags) return;
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
            if (!tag) return;
            tag.tags =tag.tags.filter(t => t.id !== variables.subTagId);
        }
    });
}
