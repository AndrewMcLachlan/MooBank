import React from "react";

import { FilterPanel } from "./FilterPanel";
import { StockTransactionList } from "./StockTransactionList";
import { StockSummary } from "./StockSummary";
import { StockHoldingPage } from "../StockHoldingPage";
import { SectionRow } from "@andrewmclachlan/mooapp";
import { Col } from "react-bootstrap";

export const StockTransactions: React.FC = () => {

    return (
        <StockHoldingPage title="Transactions">
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
