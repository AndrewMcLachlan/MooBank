import React from "react";

import { Button, Table } from "react-bootstrap";
import { IconButton, Page, Section } from "@andrewmclachlan/mooapp";
import { useAccountGroups } from "services";

import { AccountGroupRow } from "./AccountGroupRow";
import { Link, useNavigate } from "react-router-dom";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";

export const ManageAccountGroups = () => {

    const navigate = useNavigate();

    const accountGroupsQuery = useAccountGroups();

    const { data } = accountGroupsQuery;

    const accountRows: React.ReactNode[] = data?.map(a => <AccountGroupRow key={a.id} accountGroup={a} />) ?? [];

    return (
        <Page title="Account Groups" breadcrumbs={[{ text: "Account Groups", route: "/account-groups" }]} actions={[<IconButton onClick={() => navigate("/account-groups/create")} icon="plus">Create Group</IconButton>]}>
            <Table className="accounts section" hover striped>
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
