import { useForm } from "react-hook-form";
import { useNavigate } from "react-router";

import { IconButton, SectionTable, useIdParams } from "@andrewmclachlan/mooapp";

import { AccountPage, useAccount } from "components";
import * as Models from "models";
import { Controller } from "models";
import { useReprocessTransactions, useVirtualAccounts } from "services";
import { AccountForm } from "./AccountForm";

export const ManageAccount = () => {

    const reprocessTransactions = useReprocessTransactions();

    const navigate = useNavigate();

    const id = useIdParams();

    const account = useAccount();
    const { data: virtualAccounts } = useVirtualAccounts(account?.id ?? id);
  
    const getActions = (accountController: Controller) => {

        const actions = [<IconButton key="nva" onClick={() => navigate(`/accounts/${id}/manage/virtual/create`)} icon="plus">New Virtual Account</IconButton>];

        if (accountController === "Import") {
            actions.push(<IconButton key="rpt" onClick={() => reprocessTransactions(id)} icon="arrows-rotate">Reprocess Transactions</IconButton>);
        }

        return actions;
    }

    const form = useForm<Models.InstitutionAccount>({ defaultValues: account });

    return (
        <AccountPage title="Manage" breadcrumbs={[{ text: "Manage", route: `/accounts/${account?.id}/manage` }]} actions={getActions(account?.controller)}>
            <AccountForm account={account as Models.InstitutionAccount} />
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
    account: Models.InstitutionAccount;
}
