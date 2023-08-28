import * as Models from "../models";
import { useApiGet, useApiDelete, useApiDatalessPut, useHttpClient } from "@andrewmclachlan/mooapp";
import { useMutation, useQueryClient } from "react-query";

interface TagVariables {
    name: string;
}

export const useTags = () => useApiGet<Models.Tag[]>(["tags"], "api/tags");

export const useTagsHierarchy = () => useApiGet<Models.TagHierarchy>(["tags-hierarchy"], "api/tags/hierarchy");

export const useTag = (id: number) => useApiGet<Models.Tag>(["tags", id], `api/tags/${id}`);

export const useCreateTag = () => {

    const queryClient = useQueryClient();
    const httpClient = useHttpClient();

    return useMutation<Models.Tag, null, TagVariables | Models.Tag>(async (variables) => {

        const name = (variables as Models.Tag).name?.trim() ?? (variables.name).trim();
        const tags = (variables as Models.Tag).tags?.map(t => t.id) ?? [];

        return (await httpClient.put<Models.Tag>(`api/tags/${encodeURIComponent(name)}`, tags)).data;
    }, {
        onSuccess: (data: Models.Tag) => {
            const allTags = queryClient.getQueryData<Models.Tag[]>(["tags"]);
            if (!allTags) return;
            const newTags = [data, ...allTags].sort((t1, t2) => t1.name.localeCompare(t2.name));
            queryClient.setQueryData<Models.Tag[]>(["tags"], newTags);
        }
    });
}

export const useUpdateTag = () => {

    const queryClient = useQueryClient();
    const httpClient = useHttpClient();

    return useMutation<Models.Tag, null, Models.Tag>(async (variables) => {

        const name = variables.name?.trim() ?? (variables.name).trim();
        const id = variables.id;

        return (await httpClient.patch<Models.Tag>(`api/tags/${id}`, { name, excludeFromReporting: variables.settings?.excludeFromReporting, applySmoothing: variables.settings?.applySmoothing })).data;
    }, {
        onSuccess: (data: Models.Tag) => {
            queryClient.setQueryData<Models.Tag>(["tags", { id: data.id }], data);
            const allTags = queryClient.getQueryData<Models.Tag[]>(["tags"]);
            if (!allTags) return;
            
            var tagIndex = allTags.findIndex(r => r.id === data.id);

            allTags.splice(tagIndex, 1, data);

            const newTags = allTags.sort((t1, t2) => t1.name.localeCompare(t2.name));
            queryClient.setQueryData<Models.Tag[]>(["tags"], newTags);
            queryClient.invalidateQueries(["tags"]);
        }
    });
}

export const useDeleteTag = () => {

    const queryClient = useQueryClient();

    return useApiDelete<{ id: number }>((variables) => `api/tags/${variables.id}`, {
        onSuccess: (_data, variables: { id: number }) => {
            let allTags = queryClient.getQueryData<Models.Tag[]>(["tags"]);
            if (!allTags) return;
            allTags = allTags.filter(r => r.id !== (variables.id));
            allTags = allTags.sort((t1, t2) => t1.name.localeCompare(t2.name));
            queryClient.setQueryData<Models.Tag[]>(["tags"], allTags);
        }
    });
}

export const useAddSubTag = () => {

    const queryClient = useQueryClient();

    return useApiDatalessPut<Models.Tag, { tagId: number, subTagId: number }>((variables) => `api/tags/${variables.tagId}/tags/${variables.subTagId}`, {
        onSuccess: (data: Models.Tag) => {
            queryClient.setQueryData<Models.Tag>(["tags", { id: data.id }], data);
            const allTags = queryClient.getQueryData<Models.Tag[]>(["tags"]);
            if (!allTags) return;
            
            var tagIndex = allTags.findIndex(r => r.id === data.id);

            allTags.splice(tagIndex, 1, data);

            const newTags = allTags.sort((t1, t2) => t1.name.localeCompare(t2.name));
            queryClient.setQueryData<Models.Tag[]>(["tags"], newTags);
        }
        /*onSuccess: (data: Models.TransactionTag, variables) => {
            queryClient.setQueryData<Models.TransactionTag>(["tags", { id: variables.tagId }], data);
        }*/
    });
}

export const useRemoveSubTag = () => {

    const queryClient = useQueryClient();

    return useApiDelete<{ tagId: number, subTagId: number }>((variables) => `api/tags/${variables.tagId}/tags/${variables.subTagId}`, {
        onSuccess: (data: Models.Tag) => {
            queryClient.setQueryData<Models.Tag>(["tags", { id: data.id }], data);
            const allTags = queryClient.getQueryData<Models.Tag[]>(["tags"]);
            if (!allTags) return;
            
            var tagIndex = allTags.findIndex(r => r.id === data.id);

            allTags.splice(tagIndex, 1, data);

            const newTags = allTags.sort((t1, t2) => t1.name.localeCompare(t2.name));
            queryClient.setQueryData<Models.Tag[]>(["tags"], newTags);
        }
        /*onSuccess: (data: null, variables) => {
            const tag = queryClient.getQueryData<Models.TransactionTag>(["tags", { id: variables.tagId }]);
            if (!tag) return;
            tag.tags = tag.tags.filter(t => t.id !== variables.subTagId);
        }*/
    });
}
