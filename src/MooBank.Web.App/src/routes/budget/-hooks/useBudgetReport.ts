import { useQuery } from "@tanstack/react-query";
import { getBudgetReportOptions } from "api/@tanstack/react-query.gen";

export const useBudgetReport = (year: number) => useQuery({ ...getBudgetReportOptions({ path: { year } }) });
