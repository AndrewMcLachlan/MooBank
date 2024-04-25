import React from "react";

import { IconButton, LoadingTableRows, Page } from "@andrewmclachlan/mooapp";
import { Table } from "react-bootstrap";
import { useAccountGroups } from "services";

import { useNavigate } from "react-router-dom";
import { AccountGroupRow } from "./AccountGroupRow";

export const ManageAccountGroups = () => {

    const navigate = useNavigate();

    const accountGroupsQuery = useAccountGroups();

    const { data } = accountGroupsQuery;

    const accountRows: React.ReactNode[] = data?.map(a => <AccountGroupRow key={a.id} accountGroup={a} />) ?? [<LoadingTableRows key={1} rows={2} cols={3} />];

    return (
        <Page title="Groups" breadcrumbs={[{ text: "Groups", route: "/groups" }]} actions={[<IconButton key="add" onClick={() => navigate("/groups/create")} icon="plus">Create Group</IconButton>]}>
            <Table className="accounts section" hover striped>
                <thead>
                    <tr>
                        <th className="column-25">Name</th>
                        <th>Description</th>
                        <th className="column-10 row-action">Show Total</th>
                    </tr>
                </thead>
                <tbody>
                    {accountRows}
                </tbody>
            </Table>
        </Page>
    );
}
