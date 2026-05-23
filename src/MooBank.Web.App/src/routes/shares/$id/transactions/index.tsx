import React from "react";
import { createFileRoute } from "@tanstack/react-router";

import { IconLinkButton } from "@andrewmclachlan/moo-ds";

import { FilterPanel } from "./-components/FilterPanel";
import { StockHoldingCard } from "./-components/StockHoldingCard";
import { StockTransactionList } from "./-components/StockTransactionList";
import { StockHoldingPage } from "../../-components/StockHoldingPage";
import { useStockHolding } from "../../-components/StockHoldingProvider";

export const Route = createFileRoute("/shares/$id/transactions/")({
    component: StockTransactions,
});

function StockTransactions() {

    const stockHolding = useStockHolding();

    if (!stockHolding) return null;

    const actions = stockHolding.controller === "Manual"
        ? [<IconLinkButton key="add" variant="primary" icon="plus" to={`/shares/${stockHolding.id}/transactions/add`}>Add Transaction</IconLinkButton>]
        : [];

    return (
        <StockHoldingPage title="Transactions" actions={actions}>
            <StockHoldingCard />
            <FilterPanel />
            <StockTransactionList />
        </StockHoldingPage>
    );
}
