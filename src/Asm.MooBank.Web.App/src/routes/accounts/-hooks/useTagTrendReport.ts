import { useQuery } from "@tanstack/react-query";
import { tagTrendReportOptions } from "api/@tanstack/react-query.gen";
import { formatISODate } from "helpers/dateFns";
import { TrendReportSettings } from "helpers/reports";
import { transactionTypeFilter } from "store/state";

export const useTagTrendReport = (accountId: string, start: Date, end: Date, reportType: transactionTypeFilter, tagId: number, settings: TrendReportSettings) =>
    useQuery({
        ...tagTrendReportOptions({ path: { accountId, start: formatISODate(start), end: formatISODate(end), reportType: reportType.toLowerCase() as any, tagId }, query: { ApplySmoothing: settings.applySmoothing } }),
    });
