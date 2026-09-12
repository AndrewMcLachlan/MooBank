import React from "react";
import { createFileRoute } from "@tanstack/react-router";


import { FilterPanel } from "./-components/FilterPanel";
import { StockHoldingCard } from "./-components/StockHoldingCard";
import { StockTransactionList } from "./-components/StockTransactionList";
import { StockHoldingPage } from "../../-components/StockHoldingPage";
import { useStockHolding } from "../../-components/StockHoldingProvider";
import { validateStockTransactionSearch } from "./-stockTransactionSearch";
import type { PageAction } from "@andrewmclachlan/moo-app";

export const Route = createFileRoute("/shares/$id/transactions/")({
    validateSearch: validateStockTransactionSearch,
    component: StockTransactions,
});

function StockTransactions() {

    const stockHolding = useStockHolding();

    if (!stockHolding) return null;

    const actions: PageAction[] = stockHolding.controller === "Manual"
        ? [{ id: "add", label: "Add Transaction", icon: "plus", variant: "primary", group: "write", to: `/shares/${stockHolding.id}/transactions/add` }]
        : [];

    return (
        <StockHoldingPage title="Transactions" actions={actions}>
            <StockHoldingCard />
            <FilterPanel />
            <StockTransactionList />
        </StockHoldingPage>
    );
}
