import type { Tag } from "api/types.gen";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createTagByNameMutation, getTagsQueryKey } from "api/@tanstack/react-query.gen";
import { toast } from "@andrewmclachlan/moo-ds";

export const useCreateTag = () => {

    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...createTagByNameMutation(),
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: getTagsQueryKey() });
        }
    });

    const wrappedMutateAsync = (variables: { name: string } | Tag): Promise<Tag> => {
        const name = (variables as Tag).name?.trim() ?? (variables as { name: string }).name.trim();
        const tags = (variables as Tag).tags?.map(t => t.id) ?? [];
        // The generated client URL-encodes path parameters; encoding here would double-encode the name.
        return toast.promise(mutateAsync({ body: tags, path: { name } } as any) as Promise<Tag>, { pending: "Creating tag", success: "Tag created", error: "Failed to create tag" });
    };

    return {
        ...rest,
        mutate: (variables: { name: string } | Tag) => {
            wrappedMutateAsync(variables);
        },
        mutateAsync: wrappedMutateAsync,
    };
}
