import React from "react";

import { FilterPanel } from "./FilterPanel";
import { TransactionList } from "./TransactionList";
import { AccountPage, AccountSummary } from "../../components";
import { useAccount } from "components";
import { IconLinkButton } from "@andrewmclachlan/mooapp";
import { InstitutionAccount } from "models";
import { useAccountRoute } from "hooks/useAccountRoute";

export const Transactions: React.FC = () => {

    const account = useAccount();
    const route = useAccountRoute();
    
    if (!account) return null;

    const actions = !(account as InstitutionAccount).importerTypeId ? [<IconLinkButton key="import" variant="primary" icon="plus" to={`${route}/transactions/add`} relative="route">Add Transaction</IconLinkButton>] : [];

    return (
        <AccountPage title="Transactions" actions={actions}>
            <div className="section-group transactions-header">
                <AccountSummary />
                <FilterPanel />
            </div>
            <TransactionList />
        </AccountPage>
    );
}
