import { useQuery } from "@tanstack/react-query";
import { byTagReportOptions } from "api/@tanstack/react-query.gen";
import { formatISODate } from "utils/dateFns";
import { transactionTypeFilter } from "store/state";

export const useByTagReport = (accountId: string, start: Date, end: Date, reportType: transactionTypeFilter) =>
    useQuery({
        ...byTagReportOptions({ path: { accountId, start: formatISODate(start), end: formatISODate(end), reportType: reportType.toLowerCase() as any, parentTagId: 0 } }),
        enabled: !!start && !!end,
    });
