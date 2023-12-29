import { UseQueryResult } from "@tanstack/react-query";
import { trimEnd, useApiGet } from "@andrewmclachlan/mooapp";
import { AllTagAverageReport, ByTagReport, InOutReport, InOutTrendReport, ReportType, TagTrendReport, TagTrendReportSettings } from "../models/reports";
import { allTime, formatISODate } from "../helpers/dateFns";

export const reportsKey = "reports";

export const useInOutReport = (accountId: string, start: Date, end: Date) => useApiGet<InOutReport>([reportsKey, accountId, "in-out", start, end], trimEnd("/", `api/accounts/${accountId}/reports/in-out${datesToUrl(start, end)}`), { enabled: (!!start && !!end) });

export const useInOutTrendReport = (accountId: string, start: Date, end: Date) => useApiGet<InOutTrendReport>([reportsKey, accountId, "in-out-trend", start, end], trimEnd("/", `api/accounts/${accountId}/reports/in-out-trend${datesToUrl(start, end)}`), { enabled: (!!start && !!end) });

export const useBreakdownReport = (accountId: string, start: Date, end: Date, reportType: ReportType, tagId?: number) => useApiGet<ByTagReport>([reportsKey, accountId, "breakdown", reportType, start, end, tagId], trimEnd("/", `api/accounts/${accountId}/reports/${ReportType[reportType].toLowerCase()}/breakdown${datesToUrl(start, end)}/${tagId ?? ""}`), { enabled: (!!start && !!end) });

export const useByTagReport = (accountId: string, start: Date, end: Date, reportType: ReportType) => useApiGet<ByTagReport>([reportsKey, accountId, "by-tag", reportType, start, end], trimEnd("/", `api/accounts/${accountId}/reports/${ReportType[reportType].toLowerCase()}/tags${datesToUrl(start, end)}`), { enabled: (!!start && !!end) });

export const useTagTrendReport = (accountId: string, start: Date, end: Date, reportType: ReportType, tagId: number, settings: TagTrendReportSettings) => useApiGet<TagTrendReport>([reportsKey, accountId, "tag-trend", reportType, start, end, tagId, settings], trimEnd("/", `api/accounts/${accountId}/reports/${ReportType[reportType].toLowerCase()}/tag-trend${datesToUrl(start, end)}/${tagId}${toQuery(settings)}`));

export const useAllTagAverageReport = (accountId: string, start: Date, end: Date, reportType: ReportType) => useApiGet<AllTagAverageReport>([reportsKey, accountId, "all-tag-average", reportType, start, end], trimEnd("/", `api/accounts/${accountId}/reports/${ReportType[reportType].toLowerCase()}/all-tag-average${datesToUrl(start, end)}`));

const toQuery = (settings: TagTrendReportSettings) => {

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