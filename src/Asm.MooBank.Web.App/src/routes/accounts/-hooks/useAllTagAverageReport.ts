import { useQuery } from "@tanstack/react-query";
import { allTagAverageReportOptions } from "api/@tanstack/react-query.gen";
import { formatISODate } from "helpers/dateFns";
import { reportInterval } from "helpers/reports";
import { transactionTypeFilter } from "store/state";

export const useAllTagAverageReport = (accountId: string, start: Date, end: Date, reportType: transactionTypeFilter, top: number = 20, interval: reportInterval = "Monthly") =>
    useQuery({
        ...allTagAverageReportOptions({ path: { accountId, start: formatISODate(start), end: formatISODate(end), reportType: reportType.toLowerCase() as any }, query: { Top: top, Interval: interval } }),
        enabled: !!start && !!end,
    });
