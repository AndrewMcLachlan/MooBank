import React, { useContext } from "react";

import { LogicalAccount, VirtualInstrument, emptyAccount } from "models";

export interface AccountProviderProps {
    account: LogicalAccount | VirtualInstrument;
}

export const AccountContext = React.createContext<LogicalAccount | VirtualInstrument>(emptyAccount);
export const AccountProvider: React.FC<React.PropsWithChildren<AccountProviderProps>> = ({ account, children }) => {

    return (
        <AccountContext.Provider value={account}>
            {children}
        </AccountContext.Provider>
    );
};

export const useAccount = () => useContext(AccountContext);
