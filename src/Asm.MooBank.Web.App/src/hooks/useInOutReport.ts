import { useQuery } from "@tanstack/react-query";
import { inOutReportOptions } from "api/@tanstack/react-query.gen";
import { formatISODate } from "utils/dateFns";

export const useInOutReport = (accountId: string, start: Date, end: Date) =>
    useQuery({
        ...inOutReportOptions({ path: { accountId, start: formatISODate(start), end: formatISODate(end) } }),
        enabled: !!start && !!end,
    });
