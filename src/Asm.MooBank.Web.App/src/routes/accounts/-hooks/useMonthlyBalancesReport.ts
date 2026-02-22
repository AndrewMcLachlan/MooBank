import { useQuery } from "@tanstack/react-query";
import { monthlyBalancesReportOptions } from "api/@tanstack/react-query.gen";
import { formatISODate } from "utils/dateFns";

export const useMonthlyBalancesReport = (accountId: string, start: Date, end: Date) =>
    useQuery({
        ...monthlyBalancesReportOptions({ path: { accountId, start: formatISODate(start), end: formatISODate(end) } }),
        enabled: !!start && !!end,
    });
