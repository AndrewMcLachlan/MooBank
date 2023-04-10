import { useApiGet } from "@andrewmclachlan/mooapp";
import { ByTagReport, InOutReport, InOutTrendReport, ReportType } from "../models/reports";
import { formatISODate } from "../helpers/dateFns";
import { TagTrendReport } from "../models/reports/TagTrendReport";

export const reportsKey = "reports";

export const useInOutReport = (accountId: string, start: Date, end: Date) => useApiGet<InOutReport>([reportsKey, accountId, "in-out", start, end], `api/accounts/${accountId}/reports/in-out/${formatISODate(start)}/${formatISODate(end)}`);

export const useInOutTrendReport = (accountId: string, start: Date, end: Date) => useApiGet<InOutTrendReport>([reportsKey, accountId, "in-out-trend", start, end], `api/accounts/${accountId}/reports/in-out-trend/${formatISODate(start)}/${formatISODate(end)}`);

export const useBreakdownReport = (accountId: string, start: Date, end: Date, reportType: ReportType, tagId?: number) => useApiGet<ByTagReport>([reportsKey, accountId, "breakdown", reportType, start, end, tagId], `api/accounts/${accountId}/reports/${ReportType[reportType].toLowerCase()}/breakdown/${formatISODate(start)}/${formatISODate(end)}/${tagId ?? ""}`);

export const useByTagReport = (accountId: string, start: Date, end: Date, reportType: ReportType) => useApiGet<ByTagReport>([reportsKey, accountId, "by-tag", reportType, start, end], `api/accounts/${accountId}/reports/${ReportType[reportType].toLowerCase()}/tags/${formatISODate(start)}/${formatISODate(end)}`);

export const useTagTrendReport = (accountId: string, start: Date, end: Date, reportType: ReportType, tagId: number) => useApiGet<TagTrendReport>([reportsKey, accountId, "tag-trend", reportType, start, end, tagId], `api/accounts/${accountId}/reports/${ReportType[reportType].toLowerCase()}/tag-trend/${formatISODate(start)}/${formatISODate(end)}/${tagId}`);
