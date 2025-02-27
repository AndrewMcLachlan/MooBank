import React, { PropsWithChildren, useState } from "react";

import { getNumberOfPages, Page, Pagination, useIdParams } from "@andrewmclachlan/mooapp";
import { Bill } from "models/bills";
import { useBillAccount, useBills } from "services";

import { Table } from "react-bootstrap";
import { BillDetails } from "./BillDetails";
import { BillRow } from "./BillRow";

export const Bills: React.FC<PropsWithChildren> = ({ children, ...props }) => {

    const id = useIdParams();

    const [pageNumber, setPageNumber] = useState<number>(1);
    const [pageSize, _setPageSize] = useState<number>(20);
    const [showDetails, setShowDetails] = useState(false);
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
        <Page title="Bills" actions={[]} navItems={[]} breadcrumbs={[{ text: "Bills", route: "/bills" }, { text: billAccount?.name, route: `/bills/${id}` }]}>
            <BillDetails account={billAccount} bill={selectedBill} show={showDetails} onHide={() => setShowDetails(false)} onChange={() => { }} />
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
