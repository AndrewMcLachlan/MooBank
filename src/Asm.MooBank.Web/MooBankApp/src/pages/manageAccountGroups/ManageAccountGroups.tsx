import React from "react";

import { Table } from "react-bootstrap";
import { Page } from "layouts";
import { useAccountGroups } from "services";

import { AccountGroupRow } from "./AccountGroupRow";

export const ManageAccountGroups = () => {

    const accountGroupsQuery = useAccountGroups();

    const { data } = accountGroupsQuery;

    const accountRows: React.ReactNode[] = data?.map(a => <AccountGroupRow key={a.id} accountGroup={a} />) ?? [];

    return (
        <Page title="Manage Account Groupss">
            <Page.Header title="Manage Account Groups" breadcrumbs={[["Manage Account Groups", "/account-groups"]]} menuItems={[{ text: "Create Group", route: "/account-groups/create" }]} />
            <Page.Content>
                <Table className="accounts" hover>
                    <thead>
                        <tr>
                            <th className="column-25">Name</th>
                            <th>Description</th>
                            <th className="column-10 row-action">Show Position</th>
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

ManageAccountGroups.displayName = "ManageAccounts";
