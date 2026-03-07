import { useQuery } from "@tanstack/react-query";
import { getAllForecastPlansOptions } from "api/@tanstack/react-query.gen";

export const useForecastPlans = (includeArchived: boolean = false) =>
    useQuery({
        ...getAllForecastPlansOptions({ query: { IncludeArchived: includeArchived } }),
    });
