import { createFileRoute } from "@tanstack/react-router";
import { superReturnsReportOptions } from "api/@tanstack/react-query.gen";
import { SuperReturns } from "./-components/SuperReturns";
import { warmReport } from "./-utils/warmReport";

export const Route = createFileRoute("/accounts/$id/reports/super-returns")({
    // SuperReturns keys on the account only (no date range); the range from warmReport is ignored.
    loader: ({ params }) => warmReport(params.id, ({ accountId }) => superReturnsReportOptions({ path: { accountId } })),
    component: SuperReturns,
});
