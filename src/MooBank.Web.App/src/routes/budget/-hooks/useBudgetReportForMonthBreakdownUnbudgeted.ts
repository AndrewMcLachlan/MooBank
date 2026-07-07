import { useQuery } from "@tanstack/react-query";
import { getBudgetReportBreakdownForMonthForUnbudgetedItemsOptions } from "api/@tanstack/react-query.gen";

// `month` is 1-based, matching the backend and the `$year/$month` route param.
export const useBudgetReportForMonthBreakdownUnbudgeted = (year: number, month: number) => useQuery({
    ...getBudgetReportBreakdownForMonthForUnbudgetedItemsOptions({ path: { year, month } }),
});
