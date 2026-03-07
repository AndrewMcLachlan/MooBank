import { useQuery } from "@tanstack/react-query";
import { getForecastPlanOptions } from "api/@tanstack/react-query.gen";

export const useForecastPlan = (planId: string) =>
    useQuery({
        ...getForecastPlanOptions({ path: { id: planId } }),
        enabled: !!planId,
    });
