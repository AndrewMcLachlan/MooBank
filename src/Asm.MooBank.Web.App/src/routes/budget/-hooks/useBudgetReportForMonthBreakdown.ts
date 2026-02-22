import { useQuery } from "@tanstack/react-query";
import { getBudgetReportBreakdownForMonthOptions } from "api/@tanstack/react-query.gen";

export const useBudgetReportForMonthBreakdown = (year: number, month: number) => useQuery({
    ...getBudgetReportBreakdownForMonthOptions({ path: { year, month: month + 1 } }),
});
