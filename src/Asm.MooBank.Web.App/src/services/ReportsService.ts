import { UseQueryResult } from "@tanstack/react-query";
import { trimEnd, useApiGet } from "@andrewmclachlan/mooapp";
import { AllTagAverageReport, ByTagReport, defaultSettings, InOutReport, InOutTrendReport, MonthlyBalancesReport, reportInterval, TagTrendReport, TrendReportSettings } from "../models/reports";
import { formatISODate } from "../helpers/dateFns";
import { transactionTypeFilter } from "store/state";

export const reportsKey = "reports";

export const useInOutReport = (accountId: string, start: Date, end: Date): UseQueryResult<InOutReport> => useApiGet<InOutReport>([reportsKey, accountId, "in-out", start, end], trimEnd("/", `api/accounts/${accountId}/reports/in-out${datesToUrl(start, end)}`), { enabled: (!!start && !!end) });

export const useInOutAverageReport = (accountId: string, start: Date, end: Date, interval: reportInterval = "Monthly"): UseQueryResult<InOutReport> =>
    useApiGet<InOutReport>([reportsKey, accountId, "in-out-average", start, end, interval],
        trimEnd("/", `api/accounts/${accountId}/reports/in-out-average${datesToUrl(start, end)}?interval=${interval}`), { enabled: (!!start && !!end) });

export const useInOutTrendReport = (accountId: string, start: Date, end: Date, settings = defaultSettings) =>
    useApiGet<InOutTrendReport>([reportsKey, accountId, "in-out-trend", start, end, settings],
        trimEnd("/", `api/accounts/${accountId}/reports/in-out-trend${datesToUrl(start, end)}${toQuery(settings)}`), { enabled: (!!start && !!end) });

export const useBreakdownReport = (accountId: string, start: Date, end: Date, reportType: transactionTypeFilter, tagId?: number) => useApiGet<ByTagReport>([reportsKey, accountId, "breakdown", reportType, start, end, tagId], trimEnd("/", `api/accounts/${accountId}/reports/${reportType.toLowerCase()}/breakdown${datesToUrl(start, end)}/${tagId ?? ""}`), { enabled: (!!start && !!end) });

export const useByTagReport = (accountId: string, start: Date, end: Date, reportType: transactionTypeFilter) => useApiGet<ByTagReport>([reportsKey, accountId, "by-tag", reportType, start, end], trimEnd("/", `api/accounts/${accountId}/reports/${reportType.toLowerCase()}/tags${datesToUrl(start, end)}`), { enabled: (!!start && !!end) });

export const useTagTrendReport = (accountId: string, start: Date, end: Date, reportType: transactionTypeFilter, tagId: number, settings: TrendReportSettings) => useApiGet<TagTrendReport>([reportsKey, accountId, "tag-trend", reportType, start, end, tagId, settings], trimEnd("/", `api/accounts/${accountId}/reports/${reportType.toLowerCase()}/tag-trend${datesToUrl(start, end)}/${tagId}${toQuery(settings)}`));

export const useAllTagAverageReport = (accountId: string, start: Date, end: Date, reportType: transactionTypeFilter, top: number = 20, interval: reportInterval = "Monthly") => useApiGet<AllTagAverageReport>([reportsKey, accountId, "all-tag-average", reportType, start, end], trimEnd("/", `api/accounts/${accountId}/reports/${reportType.toLowerCase()}/all-tag-average${datesToUrl(start, end)}?top=${top}&interval=${interval}`), { enabled: (!!start && !!end) });

export const useMonthlyBalancesReport = (accountId: string, start: Date, end: Date) => useApiGet<MonthlyBalancesReport>([reportsKey, accountId, "monthly-balances", start, end], trimEnd("/", `api/accounts/${accountId}/reports/monthly-balances${datesToUrl(start, end)}`), { enabled: (!!start && !!end) });

const toQuery = (settings: TrendReportSettings) => {

    if (!settings) return "";

    let query = "?";

    for(const entry of Object.entries(settings)) {
        query += `${entry[0]}=${entry[1]}&`;
    }

    return query.slice(0, -1);
}

const datesToUrl = (start?: Date, end?:Date) => {

    start = start ?? new Date();
    end = end ?? new Date();

    return `/${formatISODate(start)}/${formatISODate(end)}`;
}
