import React, { useState } from "react";
import { createFileRoute } from "@tanstack/react-router";

import { useIdParams } from "@andrewmclachlan/moo-app";
import { getNumberOfPages, Pagination } from "@andrewmclachlan/moo-ds";
import type { Bill } from "api/types.gen";
import { useGridSearch, usePageInRange } from "hooks/useGridSearch";
import { validatePageSearch } from "utils/gridSearch";
import { useBillAccount } from "../../-hooks/useBillAccount";
import { useBills } from "../../-hooks/useBills";

import { Table } from "@andrewmclachlan/moo-ds";
import { AddBill } from "../../-components/AddBill";
import { BillDetails } from "../../-components/BillDetails";
import { BillRow } from "../../-components/BillRow";
import { EditBill } from "../../-components/EditBill";
import { BillsPage } from "../../-components/BillsPage";

const pageSize = 20;

export const Route = createFileRoute("/bills/accounts/$id/")({
    validateSearch: validatePageSearch,
    component: Bills,
});

function Bills() {

    const id = useIdParams();

    const { page: pageNumber, setPage } = useGridSearch();
    const [showDetails, setShowDetails] = useState(false);
    const [showAddBill, setShowAddBill] = useState(false);
    const [editingBill, setEditingBill] = useState<Bill>(undefined);
    const [selectedBill, setSelectedBill] = useState<Bill>(undefined);

    const { data: billAccount } = useBillAccount(id);
    const pagedBills = useBills(id, pageNumber, pageSize);

    const numberOfPages = getNumberOfPages(pagedBills.data?.total ?? 0, pageSize);
    usePageInRange(pageNumber, numberOfPages, !!pagedBills.data);

    if (!pagedBills?.data) return null;

    const rowClick = (bill: Bill) => {
        setSelectedBill(bill);
        setShowDetails(true);
    }

    return (
        <BillsPage title="Bills" actions={[
            { id: "add", label: "Add Bill", icon: "plus", group: "write", onClick: () => setShowAddBill(true) },
            { id: "edit", label: "Edit Account", icon: "pen-to-square", group: "write", to: `/bills/accounts/${id}/edit` },
        ]} breadcrumbs={[{ text: "Accounts", route: "/bills/accounts" }, { text: billAccount?.name, route: `/bills/accounts/${id}` }]}>
            <AddBill accountId={id} show={showAddBill} onHide={() => setShowAddBill(false)} />
            <BillDetails account={billAccount} bill={selectedBill} show={showDetails} onHide={() => setShowDetails(false)} />
            {editingBill && <EditBill accountId={id} bill={editingBill} show onHide={() => setEditingBill(undefined)} />}
            <Table striped className="section">
                <thead>
                    <tr>
                        <th>Account</th>
                        <th>Date</th>
                        <th>Cost</th>
                        <th className="row-action column-5"></th>
                    </tr>
                </thead>
                <tbody>
                    {pagedBills.data.results.map(b => <BillRow key={b.id} account={billAccount} bill={b} onClick={rowClick} onEdit={setEditingBill} />)}
                </tbody>
                <tfoot>
                    <tr>
                        <td colSpan={2} className="page-totals">Page {pageNumber} of {numberOfPages} ({pagedBills.data.total} bills)</td>
                        <td colSpan={2}>
                            <Pagination pageNumber={pageNumber} numberOfPages={numberOfPages} onChange={(_current, newPage) => setPage(newPage)} />
                        </td>
                    </tr>
                </tfoot>
            </Table>
        </BillsPage>
    );
}
