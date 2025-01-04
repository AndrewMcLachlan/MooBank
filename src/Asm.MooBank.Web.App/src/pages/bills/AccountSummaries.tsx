import React, { PropsWithChildren, useState } from "react";

import { AccountTypeSummary, Bill } from "models/bills";
import { useBillAccountSummaries } from "services";

import { Table } from "react-bootstrap";
import { Page, SectionTable } from "@andrewmclachlan/mooapp";
import { useNavigate } from "react-router";


export const BillAccountSummaries: React.FC<PropsWithChildren> = ({ children, ...props }) => {

    const { data: billAccounts } = useBillAccountSummaries();

    const navigate = useNavigate();

    const rowClick = (bill: AccountTypeSummary) => {
        navigate(`/bills/${bill.utilityType}`);
    }

    return (
        <Page title="Bills" actions={[]} navItems={[]} breadcrumbs={[{ text: "Bills", route: "/bills" }]}>
            <SectionTable striped>
                <thead>
                    <tr>
                        <th>Type</th>
                        <th>From</th>
                        <th>Accounts</th>
                    </tr>
                </thead>
                <tbody>
                    {billAccounts?.map(b =>
                        <tr key={b.utilityType} onClick={() => rowClick(b)} className="clickable">
                            <td>{b.utilityType}</td>
                            <td>{b.from}</td>
                            <td>{b.accounts.join(", ")}</td>
                        </tr>
                        )
                    }
                </tbody>
            </SectionTable>
        </Page>
    );
}
