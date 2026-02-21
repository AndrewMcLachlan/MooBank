import { createFileRoute } from "@tanstack/react-router";
import { GroupMonthlyBalances } from "../../../groups/-components/GroupMonthlyBalances";

export const Route = createFileRoute("/accounts/$id/reports/monthly-balances")({
    component: GroupMonthlyBalances,
});
