import * as Models from "../models";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
    getTagsOptions,
    getTagsQueryKey,
    getTagHierarchyOptions,
    getTagOptions,
    createTagByNameMutation,
    updateTagMutation,
    deleteTagMutation,
    addSubTagMutation,
    removeSubTagMutation,
} from "api/@tanstack/react-query.gen";
import { UpdateTag } from "api/types.gen";

export const useTags = () => useQuery({
    ...getTagsOptions(),
    select: (data) => data as unknown as Models.Tag[],
});

export const useTagsHierarchy = () => useQuery({
    ...getTagHierarchyOptions(),
    select: (data) => data as unknown as Models.TagHierarchy,
});

export const useTag = (id: number) => useQuery({
    ...getTagOptions({ path: { id } }),
    enabled: !!id,
    select: (data) => data as unknown as Models.Tag,
});

export const useCreateTag = () => {

    const queryClient = useQueryClient();

    const { mutate, mutateAsync, ...rest } = useMutation({
        ...createTagByNameMutation(),
        onSuccess: (data) => {
            const tag = data as unknown as Models.Tag;
            const allTags = queryClient.getQueryData<Models.Tag[]>(getTagsQueryKey());
            if (!allTags) return;
            const newTags = [tag, ...allTags].sort((t1, t2) => t1.name.localeCompare(t2.name));
            queryClient.setQueryData<Models.Tag[]>(getTagsQueryKey(), newTags);
        }
    });

    const wrappedMutate = (variables: { name: string } | Models.Tag) => {
        const name = (variables as Models.Tag).name?.trim() ?? (variables as { name: string }).name.trim();
        const tags = (variables as Models.Tag).tags?.map(t => t.id) ?? [];
        mutate({ body: { tags }, path: { name: encodeURIComponent(name) } });
    };

    const wrappedMutateAsync = async (variables: { name: string } | Models.Tag): Promise<Models.Tag> => {
        const name = (variables as Models.Tag).name?.trim() ?? (variables as { name: string }).name.trim();
        const tags = (variables as Models.Tag).tags?.map(t => t.id) ?? [];
        const result = await mutateAsync({ body: { tags }, path: { name: encodeURIComponent(name) } });
        return result as unknown as Models.Tag;
    };

    return {
        mutate: wrappedMutate,
        mutateAsync: wrappedMutateAsync,
        ...rest,
    };
}

export const useUpdateTag = () => {

    const queryClient = useQueryClient();

    const { mutate, ...rest } = useMutation({
        ...updateTagMutation(),
        onSuccess: (data) => {
            const tag = data as unknown as Models.Tag;
            queryClient.setQueryData<Models.Tag>(getTagsQueryKey(), tag);
            const allTags = queryClient.getQueryData<Models.Tag[]>(getTagsQueryKey());
            if (!allTags) return;

            const tagIndex = allTags.findIndex(r => r.id === tag.id);

            allTags.splice(tagIndex, 1, tag);

            const newTags = allTags.sort((t1, t2) => t1.name.localeCompare(t2.name));
            queryClient.setQueryData<Models.Tag[]>(getTagsQueryKey(), newTags);
            queryClient.invalidateQueries({ queryKey: getTagsQueryKey() });
        }
    });

    return {
        mutate: (variables: Models.Tag) => {
            mutate({
                body: {
                    name: variables.name?.trim(),
                    colour: variables.colour as UpdateTag["colour"],
                    excludeFromReporting: variables.settings?.excludeFromReporting ?? false,
                    applySmoothing: variables.settings?.applySmoothing ?? false,
                },
                path: { id: variables.id },
            });
        },
        ...rest,
    };
}

export const useDeleteTag = () => {

    const queryClient = useQueryClient();

    return useMutation({
        ...deleteTagMutation(),
        onSuccess: (_data, variables) => {
            let allTags = queryClient.getQueryData<Models.Tag[]>(getTagsQueryKey());
            if (!allTags) return;
            allTags = allTags.filter(r => r.id !== variables.path!.id);
            allTags = allTags.sort((t1, t2) => t1.name.localeCompare(t2.name));
            queryClient.setQueryData<Models.Tag[]>(getTagsQueryKey(), allTags);
        }
    });
}

export const useAddSubTag = () => {

    const queryClient = useQueryClient();

    return useMutation({
        ...addSubTagMutation(),
        onSuccess: (data) => {
            const tag = data as unknown as Models.Tag;
            queryClient.setQueryData<Models.Tag>(getTagsQueryKey(), tag);
            const allTags = queryClient.getQueryData<Models.Tag[]>(getTagsQueryKey());
            if (!allTags) return;

            const tagIndex = allTags.findIndex(r => r.id === tag.id);

            allTags.splice(tagIndex, 1, tag);

            const newTags = allTags.sort((t1, t2) => t1.name.localeCompare(t2.name));
            queryClient.setQueryData<Models.Tag[]>(getTagsQueryKey(), newTags);
        }
    });
}

export const useRemoveSubTag = () => {

    const queryClient = useQueryClient();

    return useMutation({
        ...removeSubTagMutation(),
        onSuccess: (data) => {
            const tag = data as unknown as Models.Tag;
            queryClient.setQueryData<Models.Tag>(getTagsQueryKey(), tag);
            const allTags = queryClient.getQueryData<Models.Tag[]>(getTagsQueryKey());
            if (!allTags) return;

            const tagIndex = allTags.findIndex(r => r.id === tag.id);

            allTags.splice(tagIndex, 1, tag);

            const newTags = allTags.sort((t1, t2) => t1.name.localeCompare(t2.name));
            queryClient.setQueryData<Models.Tag[]>(getTagsQueryKey(), newTags);
        }
    });
}
