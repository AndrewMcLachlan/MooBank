import { Table } from "react-bootstrap";
import { Page } from "../../layouts";
import { useAccounts } from "../../services";

import { AccountRow } from "./AccountRow";

export const ManageAccounts = () => {

    const accountsQuery = useAccounts();

    const { data } = accountsQuery;

    /* const virtualAccountRows = [];

  if (virtualAccounts) {
      for (const account of virtualAccounts) {
          virtualAccountRows.push(<VirtualAccountRow key={account.virtualAccountId} account={account} />);
      }
  }*/

    const accountRows = [];
    if (data) {
        for (const account of data) {
            accountRows.push(<AccountRow key={account.id} account={account} />);
        }
    }

    return (
        <Page title="Manage Accounts">
            <Page.Header title="Manage Accounts" breadcrumbs={[["Manage Accounts", "./accounts"]]} menuItems={[{ text: "Create Account", route: "/accounts/create" }]} />
            <Page.Content>
                <Table className="accounts" hover>
                    <thead>
                        <tr>
                            <th>Name</th>
                            <th>Description</th>
                            <th className="type">Type</th>
                            <th className="type">Controller</th>
                            <th className="number">Current Balance</th>
                        </tr>
                    </thead>
                    <tbody>
                        {accountRows}
                    </tbody>
                </Table>
            </Page.Content>
        </Page>
    );
}

ManageAccounts.displayName = "ManageAccounts";
