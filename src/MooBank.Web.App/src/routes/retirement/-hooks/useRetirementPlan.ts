import { useQuery } from "@tanstack/react-query";
import { getRetirementPlanOptions } from "api/@tanstack/react-query.gen";

export const useRetirementPlan = (planId: string) =>
    useQuery({
        ...getRetirementPlanOptions({ path: { id: planId } }),
        enabled: !!planId,
    });
