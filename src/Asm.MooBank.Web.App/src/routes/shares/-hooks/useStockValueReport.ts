import { useQuery } from "@tanstack/react-query";
import { stockValueReportOptions } from "api/@tanstack/react-query.gen";
import { formatISODate } from "helpers/dateFns";

export const useStockValueReport = (accountId: string, start?: Date, end?: Date) => useQuery({
    ...stockValueReportOptions({ path: { instrumentId: accountId }, query: { Start: start ? formatISODate(start) : "", End: end ? formatISODate(end) : "" } }),
    enabled: (!!start && !!end),
});
