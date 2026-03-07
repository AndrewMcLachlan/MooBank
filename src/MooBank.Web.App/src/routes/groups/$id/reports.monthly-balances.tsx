import { createFileRoute } from "@tanstack/react-router";
import { GroupMonthlyBalances } from "../-components/GroupMonthlyBalances";

export const Route = createFileRoute("/groups/$id/reports/monthly-balances")({
    component: GroupMonthlyBalances,
});
