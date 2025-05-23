import React from "react";

import { IconButton, LoadingTableRows, Page, SectionTable } from "@andrewmclachlan/mooapp";
import { Table } from "react-bootstrap";
import { useGroups } from "services";

import { useNavigate } from "react-router";
import { GroupRow } from "./GroupRow";

export const ManageGroups = () => {

    const navigate = useNavigate();

    const groupsQuery = useGroups();

    const { data } = groupsQuery;

    const groupRows: React.ReactNode[] = data?.map(a => <GroupRow key={a.id} group={a} />) ?? [<LoadingTableRows key={1} rows={2} cols={3} />];

    return (
        <Page title="Groups" breadcrumbs={[{ text: "Groups", route: "/groups" }]} actions={[<IconButton key="add" onClick={() => navigate("/groups/create")} icon="plus">Create Group</IconButton>]}>
            <SectionTable hover striped>
                <thead>
                    <tr>
                        <th className="column-25">Name</th>
                        <th>Description</th>
                        <th className="column-10 row-action">Show Total</th>
                    </tr>
                </thead>
                <tbody>
                    {groupRows}
                </tbody>
            </SectionTable>
        </Page>
    );
}
