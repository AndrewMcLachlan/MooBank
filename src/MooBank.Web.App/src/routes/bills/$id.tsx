import React from "react";
import { createFileRoute } from "@tanstack/react-router";

import type { Account } from "api/types.gen";
import { useBillAccountsByType } from "./-hooks/useBillAccountsByType";

import { useIdParams } from "@andrewmclachlan/moo-app";
import { SectionTable } from "@andrewmclachlan/moo-ds";
import { useNavigate } from "@tanstack/react-router";
import { BillsPage } from "./-components/BillsPage";

export const Route = createFileRoute("/bills/$id")({
    component: BillAccounts,
});

function BillAccounts() {

    const id = useIdParams();

    const { data: billAccounts } = useBillAccountsByType(id);

    const navigate = useNavigate();

    const rowClick = (account: Account) => {
        navigate({ to: `/bills/accounts/${account.id}` });
    }

    return (
        <BillsPage title="Bills" breadcrumbs={[{ text: id, route: `/bills/${id}` }]}>
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
        </BillsPage>
    );
}
