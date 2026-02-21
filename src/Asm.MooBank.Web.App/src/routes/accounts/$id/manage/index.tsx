import { createFileRoute } from "@tanstack/react-router";
import { useForm } from "react-hook-form";
import { useNavigate } from "@tanstack/react-router";

import { useIdParams } from "@andrewmclachlan/moo-app";
import { DeleteIcon, EditColumn, Icon, IconButton, SectionTable } from "@andrewmclachlan/moo-ds";

import { AccountPage, useAccount } from "components";
import type { Controller, InstitutionAccount, LogicalAccount } from "api/types.gen";
import { useCloseVirtualAccount, useReprocessTransactions, useVirtualInstruments } from "services";
import { AccountForm } from "../../-components/AccountForm";
import { InstitutionAccountRow } from "./-components/InstitutionAccountRow";
import { useState } from "react";
import { InstitutionAccountEdit } from "./-components/InstitutionAccountEdit";
import { ReprocessModal } from "./-components/ReprocessModal";

export const Route = createFileRoute("/accounts/$id/manage/")({
    component: ManageAccount,
});

function ManageAccount() {

    const reprocessTransactions = useReprocessTransactions();
    const closeVirtualAccount = useCloseVirtualAccount();
    const [showReprocessModal, setShowReprocessModal] = useState(false);
    const navigate = useNavigate();
    const id = useIdParams();

    const [showDetails, setShowDetails] = useState(false);
    const [selectedInstitutionAccount, setSelectedInstitutionAccount] = useState<InstitutionAccount>(undefined);

    const account = useAccount() as LogicalAccount;
    const { data: virtualAccounts } = useVirtualInstruments(account?.id ?? id);

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
            <IconButton key="aba" onClick={() => navigate({ to: `/accounts/${id}/manage/bank/create` })} icon="plus">Add Bank Account</IconButton>,
            <IconButton key="ava" onClick={() => navigate({ to: `/accounts/${id}/manage/virtual/create` })} icon="plus">Add Virtual Account</IconButton>,
        ];

        if (accountController === "Import") {
            actions.push(<IconButton key="rpt" onClick={() => reprocessClick(id)} icon="arrows-rotate">Reprocess Transactions</IconButton>);
        }

        return actions;
    }

    const institutionAccountClick = (institutionAccount: InstitutionAccount) => {
        setSelectedInstitutionAccount(institutionAccount);
        setShowDetails(true);
    }

    const handleEditVirtualAccount = (virtualAccountId: string) => {
        navigate({ to: `/accounts/${account?.id}/manage/virtual/${virtualAccountId}` });
    }

    const handleCloseVirtualAccount = async (virtualAccountId: string) => {
        if (confirm("Are you sure you want to close this virtual account?")) {
            await closeVirtualAccount.mutateAsync(account?.id ?? id, virtualAccountId);
        }
    }

    const form = useForm<LogicalAccount>({ defaultValues: account });

    return (
        <AccountPage title="Manage" breadcrumbs={[{ text: "Manage", route: `/accounts/${account?.id}/manage` }]} actions={getActions(account?.controller)}>
            <AccountForm account={account as LogicalAccount} />
            <ReprocessModal show={showReprocessModal} instrumentId={account?.id ?? id} onClose={() => setShowReprocessModal(false)} />
            <SectionTable header="Bank Accounts" striped hover>
                <thead>
                    <tr>
                        <th>Bank</th>
                        <th>Name</th>
                        <th>Opened Date</th>
                        <th>Closed Date</th>
                        {account?.controller === "Import" && <th>Importer Type</th>}
                        <th className="row-action column-5"></th>
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
                        <th>Type</th>
                        <th className="row-action column-5"></th>
                    </tr>
                </thead>
                <tbody>
                    {virtualAccounts && virtualAccounts.map(a => (
                        <tr key={a.id}>
                            <td>{a.name}</td>
                            <td>{a.description}</td>
                            <td>{a.controller === "Virtual" ? "Virtual" : "Reserved Sum"}</td>
                            <td className="row-action">
                                <Icon icon="pen-to-square" title="Edit" onClick={() => handleEditVirtualAccount(a.id)} />
                                <Icon icon="door-closed" title="Close" onClick={() => handleCloseVirtualAccount(a.id)} />
                            </td>
                        </tr>
                    ))}
                </tbody>
            </SectionTable>
        </AccountPage>
    );
}
