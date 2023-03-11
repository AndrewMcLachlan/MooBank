import React, { useContext } from "react";

import { Account, emptyAccount } from "../models";

export interface AccountProviderProps {
    account: Account;
}

export const AccountContext = React.createContext<Account>(emptyAccount);
export const AccountProvider: React.FC<React.PropsWithChildren<AccountProviderProps>> = ({ account, children }) => {

    return (
        <AccountContext.Provider value={account}>
            {children}
        </AccountContext.Provider>
    );
};

export const useAccount = () => useContext(AccountContext);
