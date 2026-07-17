import { createFileRoute } from "@tanstack/react-router";
import { monthlyBalancesReportForPeriodOptions } from "api/@tanstack/react-query.gen";
import { MonthlyBalances } from "./-components/MonthlyBalances";
import { warmReport } from "./-utils/warmReport";

export const Route = createFileRoute("/accounts/$id/reports/monthly-balances")({
    loader: ({ params }) => warmReport(params.id, ({ accountId, start, end }) => monthlyBalancesReportForPeriodOptions({ path: { accountId, start, end } })),
    component: MonthlyBalances,
});
