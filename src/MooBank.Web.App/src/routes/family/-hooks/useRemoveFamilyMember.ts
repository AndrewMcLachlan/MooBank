import { useMutation, useQueryClient } from "@tanstack/react-query";
import { getMyFamilyQueryKey, removeFamilyMemberMutation } from "api/@tanstack/react-query.gen";
import { toast } from "@andrewmclachlan/moo-ds";

export const useRemoveFamilyMember = () => {

    const queryClient = useQueryClient();

    const { mutateAsync } = useMutation({
        ...removeFamilyMemberMutation(),
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: getMyFamilyQueryKey() });
        },
    });

    return (userId: string) =>
        toast.promise(mutateAsync({ path: { userId } }), { pending: "Removing family member", success: "Family member removed", error: "Failed to remove family member" });
}
