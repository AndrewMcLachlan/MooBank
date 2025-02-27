import React, { useContext } from "react";

import { BillAccount } from "models";

export interface BillProviderProps {
    BillAccount: BillAccount;
}

export const BillContext = React.createContext<BillAccount>(null);
export const BillProvider: React.FC<React.PropsWithChildren<BillProviderProps>> = ({ BillAccount, children }) => {

    return (
        <BillContext.Provider value={BillAccount}>
            {children}
        </BillContext.Provider>
    );
};

export const useBill = () => useContext(BillContext);
