import { getDateRange } from "hooks/dateRange";
import { formatISODate } from "utils/dateFns";
import { getRouterQueryClient } from "utils/routerQueryClient";

type ReportRange = { accountId: string; start: string; end: string };

// Warms a report query into the cache during route resolution. The report nav links hover-preload
// (defaultPreload: "intent"), so this runs before the click and the chart is ready on mount.
// Reports default their period from getDateRange() and format dates with formatISODate, so the warmed
// key matches what the page requests. buildOptions returns the same generated query options the
// report hook uses (accountId-only reports simply ignore start/end). No-op until the root route has
// captured the QueryClient (see utils/routerQueryClient) — i.e. on cold direct loads.
export const warmReport = (
    accountId: string,
    buildOptions: (range: ReportRange) => unknown,
): void => {
    const queryClient = getRouterQueryClient();
    if (!queryClient) return;

    const { startDate, endDate } = getDateRange();
    if (!startDate || !endDate) return;

    // Each caller builds a fully-typed generated *Options object (path/query params are checked at
    // the call site); the strict queryKey tuple typing just doesn't unify through this generic
    // boundary, so cast at the ensureQueryData edge.
    void queryClient.ensureQueryData(buildOptions({
        accountId,
        start: formatISODate(startDate),
        end: formatISODate(endDate),
    }) as any);
};
