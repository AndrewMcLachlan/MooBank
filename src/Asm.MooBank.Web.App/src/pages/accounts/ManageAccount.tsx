import { useForm } from "react-hook-form";
import { useNavigate } from "react-router";

import { useIdParams } from "@andrewmclachlan/moo-app";
import { DeleteIcon, EditColumn, Icon, IconButton, SectionTable } from "@andrewmclachlan/moo-ds";

import { AccountPage, useAccount } from "components";
import * as Models from "models";
import { Controller } from "models";
import { useReprocessTransactions, useVirtualAccounts } from "services";
import { AccountForm } from "./AccountForm";
import { InstitutionAccountRow } from "./bank/InstitutionAccountRow";
import { useState } from "react";
import { InstitutionAccountEdit } from "./bank";
import { ReprocessModal } from "./ReprocessModal";

export const ManageAccount = () => {

    const reprocessTransactions = useReprocessTransactions();
    const [showReprocessModal, setShowReprocessModal] = useState(false);
    const navigate = useNavigate();
    const id = useIdParams();

    const [showDetails, setShowDetails] = useState(false);
    const [selectedInstitutionAccount, setSelectedInstitutionAccount] = useState<Models.InstitutionAccount>(undefined);

    const account = useAccount() as Models.LogicalAccount;
    const { data: virtualAccounts } = useVirtualAccounts(account?.id ?? id);

    const reprocessClick = (instrumentId: string) => {
        if (!account) return;
        if (account.controller !== "Import") return;
        var openAccounts = account.institutionAccounts.filter(ia => ia.closedDate === null);
        if (openAccounts.length === 0) return;
        if (openAccounts.length === 1) {
            reprocessTransactions(instrumentId, openAccounts[0].id);
        } else {
            setShowReprocessModal(true);
        }
    }

    const getActions = (accountController: Controller) => {

        const actions = [
            <IconButton key="aba" onClick={() => navigate(`/accounts/${id}/manage/bank/create`)} icon="plus">Add Bank Account</IconButton>,
            <IconButton key="ava" onClick={() => navigate(`/accounts/${id}/manage/virtual/create`)} icon="plus">Add Virtual Account</IconButton>,
        ];

        if (accountController === "Import") {
            actions.push(<IconButton key="rpt" onClick={() => reprocessClick(id)} icon="arrows-rotate">Reprocess Transactions</IconButton>);
        }

        return actions;
    }

    const institutionAccountClick = (institutionAccount: Models.InstitutionAccount) => {
        setSelectedInstitutionAccount(institutionAccount);
        setShowDetails(true);
    }

    const form = useForm<Models.LogicalAccount>({ defaultValues: account });

    return (
        <AccountPage title="Manage" breadcrumbs={[{ text: "Manage", route: `/accounts/${account?.id}/manage` }]} actions={getActions(account?.controller)}>
            <AccountForm account={account as Models.LogicalAccount} />
            <ReprocessModal show={showReprocessModal} instrumentId={account?.id ?? id} onClose={() => setShowReprocessModal(false)} />
            <SectionTable header="Bank Accounts" striped hover>
                <thead>
                    <tr>
                        <th>Bank</th>
                        <th>Name</th>
                        <th>Opened Date</th>
                        <th>Closed Date</th>
                        {account?.controller === "Import" && <th>Importer Type</th>}
                        <th className="actions"></th>
                    </tr>
                </thead>
                <tbody>
                    <InstitutionAccountEdit institutionAccount={selectedInstitutionAccount} show={showDetails} onHide={() => setShowDetails(false)} onSave={() => setShowDetails(false)} />
                    {account?.institutionAccounts.map(a => (
                        <InstitutionAccountRow key={a.id} institutionAccount={a} onClick={institutionAccountClick} />
                    ))}
                </tbody>
            </SectionTable>
            <SectionTable header="Virtual Accounts" striped hover>
                <thead>
                    <tr>
                        <th>Name</th>
                        <th>Description</th>
                    </tr>
                </thead>
                <tbody>
                    {virtualAccounts && virtualAccounts.map(a => (
                        <tr key={a.id} className="clickable" onClick={() => navigate(`/accounts/${account?.id}/manage/virtual/${a.id}`)}>
                            <td>{a.name}</td>
                            <td>{a.description}</td>
                        </tr>
                    ))}
                </tbody>
            </SectionTable>
        </AccountPage>
    );
}

export interface ManageAccountProps {
    account: Models.LogicalAccount;
}
