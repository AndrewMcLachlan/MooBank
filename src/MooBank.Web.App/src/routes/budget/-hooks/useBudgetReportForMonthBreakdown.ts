import { useQuery } from "@tanstack/react-query";
import { getBudgetReportBreakdownForMonthOptions } from "api/@tanstack/react-query.gen";

// `month` is 1-based, matching the backend and the `$year/$month` route param.
export const useBudgetReportForMonthBreakdown = (year: number, month: number) => useQuery({
    ...getBudgetReportBreakdownForMonthOptions({ path: { year, month } }),
});
