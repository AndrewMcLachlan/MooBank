import { createFileRoute } from "@tanstack/react-router";
import { savingsInterestReportOptions } from "api/@tanstack/react-query.gen";
import { SavingsInterest } from "./-components/SavingsInterest";
import { warmReport } from "./-utils/warmReport";

export const Route = createFileRoute("/accounts/$id/reports/savings-interest")({
    loader: ({ params }) => warmReport(params.id, ({ accountId, start, end }) => savingsInterestReportOptions({ path: { accountId, start, end } })),
    component: SavingsInterest,
});
