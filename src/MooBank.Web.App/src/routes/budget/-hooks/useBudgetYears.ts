import { useQuery } from "@tanstack/react-query";
import { getAllBudgetYearsOptions } from "api/@tanstack/react-query.gen";

export const useBudgetYears = () => useQuery({ ...getAllBudgetYearsOptions() });
