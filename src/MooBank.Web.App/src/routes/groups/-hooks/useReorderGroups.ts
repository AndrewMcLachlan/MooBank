import { useMutation, useQueryClient } from "@tanstack/react-query";
import { reorderGroupsMutation, getAllGroupsQueryKey } from "api/@tanstack/react-query.gen";
import type { Group } from "api/types.gen";
import { toast } from "@andrewmclachlan/moo-ds";

/**
 * Saves the order the groups have been dragged into.
 *
 * The cache is written before the request goes out, or the rows would spring back to their old
 * places for as long as the round trip takes — which is the one thing a drag has to not do. The
 * server answers with the order it stored, so that is written over the top rather than refetched.
 *
 * No toast on the way through, unlike the other mutations here: a drop already shows its own
 * result, and a notification on every one of them would be noise. A failure does say so, because a
 * row quietly springing back is otherwise unexplained.
 */
export const useReorderGroups = () => {
    const queryClient = useQueryClient();
    const queryKey = getAllGroupsQueryKey();

    const { mutateAsync, ...rest } = useMutation({
        ...reorderGroupsMutation(),
        onMutate: async (variables: { body: { order: { groupIds: string[] } } }) => {
            await queryClient.cancelQueries({ queryKey });

            const previous = queryClient.getQueryData<Group[]>(queryKey);

            if (previous) {
                const byId = new Map(previous.map(g => [g.id, g]));
                const reordered = variables.body.order.groupIds
                    .map(id => byId.get(id))
                    .filter((g): g is Group => !!g);

                queryClient.setQueryData(queryKey, reordered);
            }

            return { previous };
        },
        onError: (_error, _variables, context: { previous?: Group[] } | undefined) => {
            if (context?.previous) {
                queryClient.setQueryData(queryKey, context.previous);
            }
            toast.error("Failed to reorder groups");
        },
        onSuccess: (data) => {
            queryClient.setQueryData(queryKey, data);
        },
    });

    return {
        reorder: (groupIds: string[]) => mutateAsync({ body: { order: { groupIds } } } as any),
        ...rest,
    };
};
