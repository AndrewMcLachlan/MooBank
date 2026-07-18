import React from "react";
import { useFormattedAccounts } from "hooks/useFormattedAccounts";
import { AccountListGroup } from "./AccountListGroup";
import { AccountListSkeleton } from "./AccountListSkeleton";

export const AccountList: React.FC = () => {

    const { data, isLoading } = useFormattedAccounts();

    if (isLoading) return <AccountListSkeleton />;

    return (
        <>
            {data?.groups.map((ag) =>
                <AccountListGroup group={ag} isLoading={isLoading} key={ag.name} />
            )}
        </>
    );
};

AccountList.displayName = "AccountList";
