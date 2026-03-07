import React from "react";
import { createFileRoute } from "@tanstack/react-router";

import { FilterPanel } from "./-components/FilterPanel";
import { StockTransactionList } from "./-components/StockTransactionList";
import { StockSummary } from "./-components/StockSummary";
import { StockHoldingPage } from "../../-components/StockHoldingPage";
import { IconLinkButton, SectionRow } from "@andrewmclachlan/moo-ds";
import { Col } from "@andrewmclachlan/moo-ds";
import { useStockHolding } from "../../-components/StockHoldingProvider";

export const Route = createFileRoute("/shares/$id/transactions/")({
    component: StockTransactions,
});

function StockTransactions() {

    const stockHolding = useStockHolding();

    const actions = stockHolding?.controller === "Manual" ? [
        <IconLinkButton key="add" variant="primary" icon="plus" to={`shares/${stockHolding.id}/transactions/add`}>Add Transaction</IconLinkButton>,
    ] : [];

    return (
        <StockHoldingPage title="Transactions" actions={actions}>
            <SectionRow>
                <Col xl={2} md={12} sm={12}>
                    <StockSummary />
                </Col>
                <Col xl={10} md={12} sm={12}>
                    <FilterPanel />
                </Col>
            </SectionRow>
            <StockTransactionList />
        </StockHoldingPage>
    );
}
