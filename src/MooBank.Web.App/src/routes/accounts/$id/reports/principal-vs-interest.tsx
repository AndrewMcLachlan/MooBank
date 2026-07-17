import { createFileRoute } from "@tanstack/react-router";
import { principalVsInterestReportOptions } from "api/@tanstack/react-query.gen";
import { PrincipalVsInterest } from "./-components/PrincipalVsInterest";
import { warmReport } from "./-utils/warmReport";

export const Route = createFileRoute("/accounts/$id/reports/principal-vs-interest")({
    loader: ({ params }) => warmReport(params.id, ({ accountId, start, end }) => principalVsInterestReportOptions({ path: { accountId, start, end } })),
    component: PrincipalVsInterest,
});
