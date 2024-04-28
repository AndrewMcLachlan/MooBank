import React from "react";
import { useFormattedAccounts } from "services";
import { AccountListGroup } from "./AccountListGroup";

export const AccountList: React.FC<AccountListProps> = () => {

    const { data, isLoading } = useFormattedAccounts();

    return (
        <>
            {data?.groups.map((ag, index) =>
                <AccountListGroup group={ag} isLoading={isLoading} key={index} />
            )}
        </>
    );
};

AccountList.displayName = "AccountList";

export interface AccountListProps {

}
