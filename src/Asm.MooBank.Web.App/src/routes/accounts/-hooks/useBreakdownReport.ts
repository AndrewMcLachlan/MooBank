import { useQuery } from "@tanstack/react-query";
import { tagBreakdownReportForTagOptions, tagBreakdownReportOptions } from "api/@tanstack/react-query.gen";
import { formatISODate } from "utils/dateFns";
import { transactionTypeFilter } from "store/state";

export const useBreakdownReport = (accountId: string, start: Date, end: Date, reportType: transactionTypeFilter, tagId?: number) => {
    const basePath = { accountId, start: formatISODate(start), end: formatISODate(end), reportType: reportType.toLowerCase() as any };
    const options = tagId
        ? tagBreakdownReportForTagOptions({ path: { ...basePath, parentTagId: tagId } })
        : tagBreakdownReportOptions({ path: basePath });
    return useQuery({
        ...(options as ReturnType<typeof tagBreakdownReportOptions>),
        enabled: !!start && !!end,
    });
};
