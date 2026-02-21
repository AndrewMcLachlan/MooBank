import { useQuery } from "@tanstack/react-query";
import { getBudgetAmountForTagOptions } from "api/@tanstack/react-query.gen";

export const useGetTagValue = (tagId: number) => useQuery({
    ...getBudgetAmountForTagOptions({ path: { tagId } }),
    enabled: !!tagId,
});
