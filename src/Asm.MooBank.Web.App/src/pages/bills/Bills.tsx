import React, { PropsWithChildren, useState } from "react";

import { getNumberOfPages, Page, Pagination } from "@andrewmclachlan/mooapp";
import { Bill } from "models/bills";
import { useBills } from "../../services";

import { Table } from "react-bootstrap";
import { BillDetails } from "./BillDetails";
import { BillRow } from "./BillRow";

export const Bills: React.FC<PropsWithChildren> = ({ children, ...props }) => {

    const [pageNumber, setPageNumber] = useState<number>(1);
    const [pageSize, _setPageSize] = useState<number>(20);
    const [showDetails, setShowDetails] = useState(false);
    const [selectedBill, setSelectedBill] = useState<Bill>(undefined);

    const pagedBills = useBills(pageNumber, pageSize);

    if (!pagedBills?.data) return null;

    const numberOfPages = getNumberOfPages(pagedBills.data.total, pageSize);

    const rowClick = (bill: Bill) => {
        setSelectedBill(bill);
        setShowDetails(true);
    }

    return (
        <Page title="Bills" actions={[]} navItems={[]} breadcrumbs={[{ text: "Bills", route: "/biils" }]}>
            <BillDetails bill={selectedBill} show={showDetails} onHide={() => setShowDetails(false)} onChange={() => { }} />
            <Table striped bordered={false} borderless className="section">
                <thead>
                    <tr>
                        <th>Account</th>
                        <th>Date</th>
                        <th>Cost</th>
                    </tr>
                </thead>
                <tbody>
                    {pagedBills.data.results.map(b => <BillRow key={b.id} bill={b} onClick={rowClick} />)}
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
