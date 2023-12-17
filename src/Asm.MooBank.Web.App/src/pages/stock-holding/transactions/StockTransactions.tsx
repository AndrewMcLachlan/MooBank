import React from "react";

import { FilterPanel } from "./FilterPanel";
import { StockTransactionList } from "./StockTransactionList";
import { AccountSummary } from "../../../components";
import { StockHoldingPage } from "../StockHoldingPage";

export const StockTransactions: React.FC = () => {

    return (
        <StockHoldingPage title="Transactions">
            <div className="section-group transactions-header">
                <AccountSummary />
                <FilterPanel />
            </div>
            <StockTransactionList />
        </StockHoldingPage>
    );
}
