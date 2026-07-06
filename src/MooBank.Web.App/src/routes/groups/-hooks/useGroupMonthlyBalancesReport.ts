import { useQuery } from "@tanstack/react-query";
import { groupMonthlyBalancesReportOptions } from "api/@tanstack/react-query.gen";
import { formatISODate } from "utils/dateFns";

export const useGroupMonthlyBalancesReport = (groupId: string, start: Date, end: Date) =>
    useQuery({
        ...groupMonthlyBalancesReportOptions({ path: { groupId, start: start ? formatISODate(start) : "", end: end ? formatISODate(end) : "" } }),
        enabled: !!groupId && !!start && !!end,
    });
