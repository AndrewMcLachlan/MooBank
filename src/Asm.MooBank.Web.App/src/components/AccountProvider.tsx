import React, { useContext } from "react";

import { InstitutionAccount, VirtualAccount, emptyAccount } from "models";

export interface AccountProviderProps {
    account: InstitutionAccount | VirtualAccount;
}

export const AccountContext = React.createContext<InstitutionAccount | VirtualAccount>(emptyAccount);
export const AccountProvider: React.FC<React.PropsWithChildren<AccountProviderProps>> = ({ account, children }) => {

    return (
        <AccountContext.Provider value={account}>
            {children}
        </AccountContext.Provider>
    );
};

export const useAccount = () => useContext(AccountContext);
