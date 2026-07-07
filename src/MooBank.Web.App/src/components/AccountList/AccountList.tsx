import React from "react";
import { useFormattedAccounts } from "hooks/useFormattedAccounts";
import { AccountListGroup } from "./AccountListGroup";

export const AccountList: React.FC = () => {

    const { data, isLoading } = useFormattedAccounts();

    return (
        <>
            {data?.groups.map((ag) =>
                <AccountListGroup group={ag} isLoading={isLoading} key={ag.name} />
            )}
        </>
    );
};

AccountList.displayName = "AccountList";
