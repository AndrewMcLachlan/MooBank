import { useQuery } from "@tanstack/react-query";
import { superReturnsReportOptions } from "api/@tanstack/react-query.gen";

export const useSuperReturnsReport = (accountId: string, enabled: boolean) =>
    useQuery({
        ...superReturnsReportOptions({ path: { accountId } }),
        enabled: enabled && !!accountId,
    });
