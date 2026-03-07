import { createFileRoute } from "@tanstack/react-router";
import { Transactions } from "../../-transactions/Transactions";

export const Route = createFileRoute("/accounts/$id/virtual/$virtualId/transactions")({
    component: Transactions,
});
