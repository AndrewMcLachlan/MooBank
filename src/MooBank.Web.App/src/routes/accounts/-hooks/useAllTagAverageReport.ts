import { useQuery } from "@tanstack/react-query";
import { allTagAverageReportOptions } from "api/@tanstack/react-query.gen";
import { formatISODate } from "utils/dateFns";
import type { reportInterval } from "models/reports";
import type { transactionTypeFilter } from "models/transactions";

export const useAllTagAverageReport = (accountId: string, start: Date, end: Date, reportType: transactionTypeFilter, top: number = 20, interval: reportInterval = "Monthly") =>
    useQuery({
        ...allTagAverageReportOptions({ path: { accountId, start: start ? formatISODate(start) : "", end: end ? formatISODate(end) : "", reportType: reportType.toLowerCase() as any }, query: { Top: top, Interval: interval } }),
        enabled: !!start && !!end,
    });
