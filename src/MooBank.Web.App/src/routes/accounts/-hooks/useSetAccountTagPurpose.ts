import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
    setAccountTagPurposeMutation,
    deleteAccountTagPurposeMutation,
    getAccountQueryKey,
    getAccountsQueryKey,
} from "api/@tanstack/react-query.gen";
import type { TagPurpose } from "api/types.gen";
import { toast } from "@andrewmclachlan/moo-ds";

export const useSetAccountTagPurpose = () => {
    const queryClient = useQueryClient();

    const invalidate = (instrumentId: string | undefined) => {
        queryClient.invalidateQueries({ queryKey: getAccountsQueryKey() });
        if (instrumentId) {
            queryClient.invalidateQueries({ queryKey: getAccountQueryKey({ path: { instrumentId } }) });
        }
    };

    const setMutation = useMutation({
        ...setAccountTagPurposeMutation(),
        onSettled: (_data, _error, variables) => invalidate((variables as any).path?.instrumentId),
    });

    const deleteMutation = useMutation({
        ...deleteAccountTagPurposeMutation(),
        onSettled: (_data, _error, variables) => invalidate((variables as any).path?.instrumentId),
    });

    return {
        set: (instrumentId: string, purpose: TagPurpose, tagId: number | null) =>
            tagId === null
                ? toast.promise(
                    deleteMutation.mutateAsync({ path: { instrumentId, purpose } } as any),
                    { pending: "Clearing tag", success: "Tag cleared", error: "Failed to clear tag" },
                )
                : toast.promise(
                    setMutation.mutateAsync({ path: { instrumentId, purpose, tagId } } as any),
                    { pending: "Updating tag", success: "Tag updated", error: "Failed to update tag" },
                ),
    };
};
