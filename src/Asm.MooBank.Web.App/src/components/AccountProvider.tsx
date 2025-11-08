import React, { useContext } from "react";

import { LogicalAccount, VirtualAccount, emptyAccount } from "models";

export interface AccountProviderProps {
    account: LogicalAccount | VirtualAccount;
}

export const AccountContext = React.createContext<LogicalAccount | VirtualAccount>(emptyAccount);
export const AccountProvider: React.FC<React.PropsWithChildren<AccountProviderProps>> = ({ account, children }) => {

    return (
        <AccountContext.Provider value={account}>
            {children}
        </AccountContext.Provider>
    );
};

export const useAccount = () => useContext(AccountContext);
