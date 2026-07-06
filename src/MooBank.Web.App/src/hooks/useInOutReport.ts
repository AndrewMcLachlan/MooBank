import { useQuery } from "@tanstack/react-query";
import { inOutReportOptions } from "api/@tanstack/react-query.gen";
import { formatISODate } from "utils/dateFns";

export const useInOutReport = (accountId: string, start: Date, end: Date) =>
    useQuery({
        ...inOutReportOptions({ path: { accountId, start: start ? formatISODate(start) : "", end: end ? formatISODate(end) : "" } }),
        enabled: !!accountId && !!start && !!end,
    });
