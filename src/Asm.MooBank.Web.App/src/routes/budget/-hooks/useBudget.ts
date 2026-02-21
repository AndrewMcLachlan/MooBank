import { useQuery } from "@tanstack/react-query";
import { getBudgetOptions } from "api/@tanstack/react-query.gen";

export const useBudget = (year: number) => useQuery({
    ...getBudgetOptions({ path: { year } }),
});
