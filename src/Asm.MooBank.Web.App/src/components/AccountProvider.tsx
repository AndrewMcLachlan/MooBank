import React, { useContext } from "react";

import { InstitutionAccount, emptyAccount } from "models";

export interface AccountProviderProps {
    account: InstitutionAccount;
}

export const AccountContext = React.createContext<InstitutionAccount>(emptyAccount);
export const AccountProvider: React.FC<React.PropsWithChildren<AccountProviderProps>> = ({ account, children }) => {

    return (
        <AccountContext.Provider value={account}>
            {children}
        </AccountContext.Provider>
    );
};

export const useAccount = () => useContext(AccountContext);
