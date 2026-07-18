import React from "react";
import { useFormattedAccounts } from "hooks/useFormattedAccounts";
import { AccountListGroup } from "./AccountListGroup";
import { AccountListSkeleton } from "./AccountListSkeleton";

export const AccountList: React.FC = () => {

    const { data, isLoading } = useFormattedAccounts();

    // Gate on "no data yet" rather than isLoading: a persisted query rehydrating from IndexedDB is
    // pending-but-paused (isLoading === false) while data is still undefined, so isLoading would skip
    // the skeleton on a normal refresh.
    if (!data) return <AccountListSkeleton />;

    return (
        <>
            {data?.groups.map((ag) =>
                <AccountListGroup group={ag} isLoading={isLoading} key={ag.name} />
            )}
        </>
    );
};

AccountList.displayName = "AccountList";
