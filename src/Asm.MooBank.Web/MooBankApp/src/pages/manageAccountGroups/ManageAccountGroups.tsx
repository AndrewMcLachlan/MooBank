import React from "react";

import { Button, Table } from "react-bootstrap";
import { Page, Section } from "@andrewmclachlan/mooapp";
import { useAccountGroups } from "services";

import { AccountGroupRow } from "./AccountGroupRow";
import { Link, useNavigate } from "react-router-dom";

export const ManageAccountGroups = () => {

    const navigate = useNavigate();

    const accountGroupsQuery = useAccountGroups();

    const { data } = accountGroupsQuery;

    const accountRows: React.ReactNode[] = data?.map(a => <AccountGroupRow key={a.id} accountGroup={a} />) ?? [];

    return (
        <Page title="Manage Account Groups" breadcrumbs={[{ text: "Manage Account Groups", route: "/account-groups" }]}>
            <div className="section-group">
                <Section>
                    <Button onClick={() => navigate("/account-groups/create")}>Create Group</Button>
            </Section>
            </div>
            <Table className="accounts section" hover>
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
        </Page>
    );
}

ManageAccountGroups.displayName = "ManageAccounts";
