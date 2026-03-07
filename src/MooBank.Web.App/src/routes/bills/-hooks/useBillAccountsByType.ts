import { useQuery } from "@tanstack/react-query";
import { getBillAccountsByTypeOptions } from "api/@tanstack/react-query.gen";
import type { UtilityType } from "api/types.gen";

export const useBillAccountsByType = (utilityType: string) => useQuery({
    ...getBillAccountsByTypeOptions({ path: { type: utilityType as UtilityType } }),
});
