import { useQuery } from "@tanstack/react-query";
import { inOutAverageReportOptions } from "api/@tanstack/react-query.gen";
import { formatISODate } from "utils/dateFns";
import { reportInterval } from "models/reports";

export const useInOutAverageReport = (accountId: string, start: Date, end: Date, interval: reportInterval = "Monthly") =>
    useQuery({
        ...inOutAverageReportOptions({ path: { accountId, start: formatISODate(start), end: formatISODate(end) }, query: { Interval: interval } }),
        enabled: !!start && !!end,
    });
