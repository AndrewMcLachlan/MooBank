import { useApiGet } from "@andrewmclachlan/mooapp";
import { ByTagReport, InOutReport, ReportType } from "../models/reports";
import { formatISODate } from "../helpers/dateFns";

export const reportsKey = "reports";

export const useInOutReport = (accountId: string, start: Date, end: Date) => useApiGet<InOutReport>([reportsKey, accountId, "in-out", start, end], `api/accounts/${accountId}/reports/in-out/${formatISODate(start)}/${formatISODate(end)}`);

export const useByTagReport = (accountId: string, start: Date, end: Date, reportType: ReportType, tagId?: number) => useApiGet<ByTagReport>([reportsKey, accountId, "by-tag", start, end, tagId], `api/accounts/${accountId}/reports/${ReportType[reportType].toLowerCase()}/tags/${formatISODate(start)}/${formatISODate(end)}/${tagId ?? ""}`);
