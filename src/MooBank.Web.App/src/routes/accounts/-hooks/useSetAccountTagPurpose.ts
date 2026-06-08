import { useMutation, useQueryClient } from "@tanstack/react-query";
import { setAccountTagPurposeMutation, getAccountQueryKey, getAccountsQueryKey } from "api/@tanstack/react-query.gen";
import type { TagPurpose } from "api/types.gen";
import { toast } from "@andrewmclachlan/moo-ds";

export const useSetAccountTagPurpose = () => {
    const queryClient = useQueryClient();

    const { mutateAsync, ...rest } = useMutation({
        ...setAccountTagPurposeMutation(),
        onSettled: (_data, _error, variables) => {
            const instrumentId = (variables as any).path?.instrumentId as string | undefined;
            queryClient.invalidateQueries({ queryKey: getAccountsQueryKey() });
            if (instrumentId) {
                queryClient.invalidateQueries({ queryKey: getAccountQueryKey({ path: { instrumentId } }) });
            }
        },
    });

    return {
        set: (instrumentId: string, purpose: TagPurpose, tagId: number | null) =>
            toast.promise(
                mutateAsync({
                    path: { instrumentId, purpose },
                    query: tagId !== null ? { TagId: tagId } : undefined,
                } as any),
                { pending: "Updating tag", success: "Tag updated", error: "Failed to update tag" },
            ),
        ...rest,
    };
};
