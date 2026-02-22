import React from "react";
import { createFileRoute } from "@tanstack/react-router";
import { useNavigate } from "@tanstack/react-router";

import { IconButton, SectionTable } from "@andrewmclachlan/moo-ds";

import type { Account } from "api/types.gen";
import { useBillAccounts } from "../-hooks/useBillAccounts";
import { BillsPage } from "../-components/BillsPage";

export const Route = createFileRoute("/bills/accounts/")({
    component: AllBillAccounts,
});

function AllBillAccounts() {
    const navigate = useNavigate();
    const { data: accounts } = useBillAccounts();

    const rowClick = (account: Account) => {
        navigate({ to: `/bills/accounts/${account.id}` });
    }

    return (
        <BillsPage
            title="Accounts"
            breadcrumbs={[{ text: "Accounts", route: "/bills/accounts" }]}
            actions={[
                <IconButton key="create" onClick={() => navigate({ to: "/bills/accounts/create" })} icon="plus">Add Account</IconButton>
            ]}
        >
            <SectionTable striped>
                <thead>
                    <tr>
                        <th>Name</th>
                        <th>Type</th>
                        <th>First Bill</th>
                        <th>Last Bill</th>
                    </tr>
                </thead>
                <tbody>
                    {accounts?.map(a =>
                        <tr key={a.id} onClick={() => rowClick(a)} className="clickable">
                            <td>{a.name}</td>
                            <td>{a.utilityType}</td>
                            <td>{a.firstBill}</td>
                            <td>{a.latestBill}</td>
                        </tr>
                    )}
                </tbody>
            </SectionTable>
        </BillsPage>
    );
}
