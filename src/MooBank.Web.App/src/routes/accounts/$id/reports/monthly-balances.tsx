import { createFileRoute } from "@tanstack/react-router";
import { MonthlyBalances } from "./-components/MonthlyBalances";

export const Route = createFileRoute("/accounts/$id/reports/monthly-balances")({
    component: MonthlyBalances,
});
