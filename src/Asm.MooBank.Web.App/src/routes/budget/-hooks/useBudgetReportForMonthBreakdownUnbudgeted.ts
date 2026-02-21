import { useQuery } from "@tanstack/react-query";
import { getBudgetReportBreakdownForMonthForUnbudgetedItemsOptions } from "api/@tanstack/react-query.gen";

export const useBudgetReportForMonthBreakdownUnbudgeted = (year: number, month: number) => useQuery({
    ...getBudgetReportBreakdownForMonthForUnbudgetedItemsOptions({ path: { year, month: month + 1 } }),
});
