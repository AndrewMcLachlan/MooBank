import React, { PropsWithChildren } from "react";

import { AccountTypeSummary, BillAccount } from "models/bills";
import { useBillAccountsByType } from "services";

import { Page, SectionTable, useIdParams } from "@andrewmclachlan/mooapp";
import { useNavigate } from "react-router";


export const BillAccounts: React.FC<PropsWithChildren> = ({ children, ...props }) => {

    const id = useIdParams();

    const { data: billAccounts } = useBillAccountsByType(id);

    const navigate = useNavigate();

    const rowClick = (bill: BillAccount) => {
        navigate(`/bills/accounts/${bill.id}`);
    }

    return (
        <Page title="Bills" actions={[]} navItems={[]} breadcrumbs={[{ text: "Bills", route: "/bills" }, { text: id, route: `/bills/${id}` }]}>
            <SectionTable striped>
                <thead>
                    <tr>
                        <th>Name</th>
                        <th>Type</th>
                        <th>From</th>
                        <th>Latest</th>
                    </tr>
                </thead>
                <tbody>
                    {billAccounts?.map(b =>
                        <tr key={b.id} onClick={() => rowClick(b)} className="clickable">
                            <td>{b.name}</td>
                            <td>{b.utilityType}</td>
                            <td>{b.firstBill}</td>
                            <td>{b.latestBill}</td>
                        </tr>
                        )
                    }
                </tbody>
            </SectionTable>
        </Page>
    );
}
