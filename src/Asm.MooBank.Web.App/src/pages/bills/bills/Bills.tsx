import React, { PropsWithChildren, useState } from "react";

import { Page, useIdParams } from "@andrewmclachlan/moo-app";
import { getNumberOfPages, IconButton, Pagination } from "@andrewmclachlan/moo-ds";
import { Bill } from "models/bills";
import { useBillAccount, useBills } from "services";

import { Button, Table } from "react-bootstrap";
import { AddBill } from "./AddBill";
import { BillDetails } from "./BillDetails";
import { BillRow } from "./BillRow";

export const Bills: React.FC<PropsWithChildren> = ({ children, ...props }) => {

    const id = useIdParams();

    const [pageNumber, setPageNumber] = useState<number>(1);
    const [pageSize, _setPageSize] = useState<number>(20);
    const [showDetails, setShowDetails] = useState(false);
    const [showAddBill, setShowAddBill] = useState(false);
    const [selectedBill, setSelectedBill] = useState<Bill>(undefined);

    const { data: billAccount } = useBillAccount(id);
    const pagedBills = useBills(id, pageNumber, pageSize);

    if (!pagedBills?.data) return null;

    const numberOfPages = getNumberOfPages(pagedBills.data.total, pageSize);

    const rowClick = (bill: Bill) => {
        setSelectedBill(bill);
        setShowDetails(true);
    }

    return (
        <Page title="Bills" actions={[<IconButton key="add" onClick={() => setShowAddBill(true)} icon="plus">Add Bill</IconButton>]} navItems={[]} breadcrumbs={[{ text: "Bills", route: "/bills" }, { text: "Accounts", route: "/bills/accounts" }, { text: billAccount?.name, route: `/bills/accounts/${id}` }]}>
            <AddBill accountId={id} show={showAddBill} onHide={() => setShowAddBill(false)} />
            <BillDetails account={billAccount} bill={selectedBill} show={showDetails} onHide={() => setShowDetails(false)} />
            <Table striped className="section">
                <thead>
                    <tr>
                        <th>Account</th>
                        <th>Date</th>
                        <th>Cost</th>
                    </tr>
                </thead>
                <tbody>
                    {pagedBills.data.results.map(b => <BillRow key={b.id} account={billAccount} bill={b} onClick={rowClick} />)}
                </tbody>
                <tfoot>
                    <tr>
                        <td colSpan={2} className="page-totals">Page {pageNumber} of {numberOfPages} ({pagedBills.data.total} bills)</td>
                        <td colSpan={2}>
                            <Pagination pageNumber={pageNumber} numberOfPages={numberOfPages} onChange={(_current, newPage) => setPageNumber(newPage)} />
                        </td>
                    </tr>
                </tfoot>
            </Table>
        </Page>
    );
}
