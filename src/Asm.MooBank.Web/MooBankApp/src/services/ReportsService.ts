import { useApiGet } from "@andrewmclachlan/mooapp";
import format from "date-fns/format";
import { InOutReport } from "../models/reports";

export const reportsKey = "reports";

export const useInOutReport = (accountId: string, start: Date, end: Date) => useApiGet<InOutReport>([reportsKey, accountId, "in-out", start, end], `api/accounts/${accountId}/reports/in-out/${format(start, "yyyy-MM-dd")}/${format(end, "yyyy-MM-dd")}`);
