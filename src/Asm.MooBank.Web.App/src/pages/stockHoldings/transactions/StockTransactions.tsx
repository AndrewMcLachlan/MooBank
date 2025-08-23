import React from "react";

import { FilterPanel } from "./FilterPanel";
import { StockTransactionList } from "./StockTransactionList";
import { StockSummary } from "./StockSummary";
import { StockHoldingPage } from "../StockHoldingPage";
import { IconLinkButton, SectionRow } from "@andrewmclachlan/moo-ds";
import { Col } from "react-bootstrap";
import { useStockHolding } from "../StockHoldingProvider";

export const StockTransactions: React.FC = () => {

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
