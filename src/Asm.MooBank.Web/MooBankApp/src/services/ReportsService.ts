import { useApiGet } from "@andrewmclachlan/mooapp";
import { ByTagReport, InOutReport, ReportType } from "../models/reports";
import { formatISODate } from "../helpers/dateFns";

export const reportsKey = "reports";

export const useInOutReport = (accountId: string, start: Date, end: Date) => useApiGet<InOutReport>([reportsKey, accountId, "in-out", start, end], `api/accounts/${accountId}/reports/in-out/${formatISODate(start)}/${formatISODate(end)}`);

export const useBreakdownReport = (accountId: string, start: Date, end: Date, reportType: ReportType, tagId?: number) => useApiGet<ByTagReport>([reportsKey, accountId, "breakdown", start, end, tagId], `api/accounts/${accountId}/reports/${ReportType[reportType].toLowerCase()}/breakdown/${formatISODate(start)}/${formatISODate(end)}/${tagId ?? ""}`);

export const useByTagReport = (accountId: string, start: Date, end: Date, reportType: ReportType) => useApiGet<ByTagReport>([reportsKey, accountId, "by-tag", start, end], `api/accounts/${accountId}/reports/${ReportType[reportType].toLowerCase()}/tags/${formatISODate(start)}/${formatISODate(end)}`);
