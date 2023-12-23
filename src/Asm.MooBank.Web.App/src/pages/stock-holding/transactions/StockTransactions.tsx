import React from "react";

import { FilterPanel } from "./FilterPanel";
import { StockTransactionList } from "./StockTransactionList";
import { StockSummary } from "./StockSummary";
import { StockHoldingPage } from "../StockHoldingPage";

export const StockTransactions: React.FC = () => {

    return (
        <StockHoldingPage title="Transactions">
            <div className="section-group transactions-header">
                <StockSummary />
                <FilterPanel />
            </div>
            <StockTransactionList />
        </StockHoldingPage>
    );
}
