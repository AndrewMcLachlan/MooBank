import React from "react";

import { Table } from "react-bootstrap";
import { Page } from "@andrewmclachlan/mooapp";
import { useAccounts } from "services";

import { AccountRow } from "./AccountRow";

export const ManageAccounts = () => {

    const accountsQuery = useAccounts();

    const { data } = accountsQuery;

    const accountRows: React.ReactNode[] = data?.map(a => <AccountRow key={a.id} account={a} />) ?? [];

    return (
        <Page title="Manage Accounts" breadcrumbs={[{ text: "Manage Accounts", route: "/accounts" }]} navItems={[{ text: "Create Account", route: "/accounts/create" }, { text: "Create Shares", route: "/shares/create" }]}>
            <Table className="accounts" hover>
                <thead>
                    <tr>
                        <th>Name</th>
                        <th>Description</th>
                        <th className="type">Type</th>
                        <th className="type">Controller</th>
                        <th className="number">Balance</th>
                    </tr>
                </thead>
                <tbody>
                    {accountRows}
                </tbody>
            </Table>
        </Page>
    );
}

ManageAccounts.displayName = "ManageAccounts";
