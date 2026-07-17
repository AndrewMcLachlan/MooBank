import { createFileRoute } from "@tanstack/react-router";
import { superContributionsReportOptions } from "api/@tanstack/react-query.gen";
import { SuperContributions } from "./-components/SuperContributions";
import { warmReport } from "./-utils/warmReport";

export const Route = createFileRoute("/accounts/$id/reports/super-contributions")({
    loader: ({ params }) => warmReport(params.id, ({ accountId, start, end }) => superContributionsReportOptions({ path: { accountId, start, end } })),
    component: SuperContributions,
});
