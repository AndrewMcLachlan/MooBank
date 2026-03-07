import React, { useContext } from "react";

import type { Account } from "api/types.gen";

export interface BillProviderProps {
    BillAccount: Account;
}

export const BillContext = React.createContext<Account>(null);
export const BillProvider: React.FC<React.PropsWithChildren<BillProviderProps>> = ({ BillAccount, children }) => {

    return (
        <BillContext.Provider value={BillAccount}>
            {children}
        </BillContext.Provider>
    );
};

export const useBill = () => useContext(BillContext);
