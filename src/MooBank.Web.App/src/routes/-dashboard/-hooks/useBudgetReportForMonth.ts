import { useQuery } from "@tanstack/react-query";
import { getBudgetReportForMonthOptions } from "api/@tanstack/react-query.gen";

export const useBudgetReportForMonth = (year: number, month: number) => useQuery({
    ...getBudgetReportForMonthOptions({ path: { year, month: month + 1 } }),
});
