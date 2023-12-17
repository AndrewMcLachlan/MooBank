import React, { useContext } from "react";

import { StockHolding } from "models";

export interface StockHoldingProviderProps {
    stockHolding: StockHolding;
}

export const StockHoldingContext = React.createContext<StockHolding>(undefined);
export const StockHoldingProvider: React.FC<React.PropsWithChildren<StockHoldingProviderProps>> = ({ stockHolding: account, children }) => {

    return (
        <StockHoldingContext.Provider value={account}>
            {children}
        </StockHoldingContext.Provider>
    );
};

export const useStockHolding = () => useContext(StockHoldingContext);
