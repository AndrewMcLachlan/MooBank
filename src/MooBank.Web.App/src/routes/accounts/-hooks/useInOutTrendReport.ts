import { useQuery } from "@tanstack/react-query";
import { inOutTrendReportOptions } from "api/@tanstack/react-query.gen";
import { formatISODate } from "utils/dateFns";
import { TrendReportSettings, defaultSettings } from "models/reports";

export const useInOutTrendReport = (accountId: string, start: Date, end: Date, settings: TrendReportSettings = defaultSettings) =>
    useQuery({
        ...inOutTrendReportOptions({ path: { accountId, start: formatISODate(start), end: formatISODate(end) }, query: { Interval: settings.interval } }),
        enabled: !!start && !!end,
    });
