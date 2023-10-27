import "./AccountList.scss";

import React from "react";
import { useFormattedAccounts } from "services";
import { AccountListGroup } from "./AccountListGroup";

export const AccountList: React.FC<AccountListProps> = (_props) => {

    const { data, isLoading } = useFormattedAccounts();

    return (
        <>
            {data?.accountGroups.map((ag, index) =>
                <AccountListGroup accountGroup={ag} isLoading={isLoading} key={index} />
            )}
        </>
    );
};

AccountList.displayName = "AccountList";

export interface AccountListProps {

}