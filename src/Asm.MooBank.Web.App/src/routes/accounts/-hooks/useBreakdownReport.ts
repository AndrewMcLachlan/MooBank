import { useQuery } from "@tanstack/react-query";
import { tagBreakdownReportOptions } from "api/@tanstack/react-query.gen";
import { formatISODate } from "utils/dateFns";
import { transactionTypeFilter } from "store/state";

export const useBreakdownReport = (accountId: string, start: Date, end: Date, reportType: transactionTypeFilter, tagId?: number) =>
    useQuery({
        ...tagBreakdownReportOptions({ path: { accountId, start: formatISODate(start), end: formatISODate(end), reportType: reportType.toLowerCase() as any, parentTagId: tagId ?? 0 } }),
        enabled: !!start && !!end,
    });
