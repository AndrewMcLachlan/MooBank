import { createFileRoute } from "@tanstack/react-router";
import { Transactions } from "../../-transactions/Transactions";
import { validateTransactionSearch } from "../../-transactions/transactionSearch";

export const Route = createFileRoute("/accounts/$id/virtual/$virtualId/transactions")({
    validateSearch: validateTransactionSearch,
    component: Transactions,
});
