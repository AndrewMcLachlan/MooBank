import React, { useState } from "react";
import { Table } from "@andrewmclachlan/moo-ds";
import { format, parseISO, subYears } from "date-fns";
import { getNumberOfPages, Pagination, useLocalStorage } from "@andrewmclachlan/moo-ds";

import { Bill, BillAccount, UtilityType } from "models/bills";
import { useBillsByUtilityType, useBillAccountsByType, BillFilter } from "services/BillService";
import { BillDetails } from "../bills/BillDetails";
import { BillFilterPanel } from "./BillFilterPanel";
import { BillsChart, UsageChart } from "../reports";
import { getUnit } from "helpers/units";

export interface UtilityTypeBillsTabProps {
    utilityType: UtilityType;
}

const formatCurrency = (value: number) => {
    return new Intl.NumberFormat("en-AU", { style: "currency", currency: "AUD" }).format(value);
};

const getDefaultFilter = (): BillFilter => ({
    startDate: format(subYears(new Date(), 2), "yyyy-MM-dd"),
    endDate: format(new Date(), "yyyy-MM-dd"),
});

export const UtilityTypeBillsTab: React.FC<UtilityTypeBillsTabProps> = ({ utilityType }) => {
    const [pageNumber, setPageNumber] = useState<number>(1);
    const [pageSize] = useState<number>(20);
    const [filter, setFilter] = useLocalStorage<BillFilter>("bills-filter", getDefaultFilter());
    const [showDetails, setShowDetails] = useState(false);
    const [selectedBill, setSelectedBill] = useState<Bill | undefined>(undefined);
    const [selectedAccount, setSelectedAccount] = useState<BillAccount | undefined>(undefined);

    const { data: accounts } = useBillAccountsByType(utilityType);
    const { data: pagedBills } = useBillsByUtilityType(utilityType, pageNumber, pageSize, filter);

    const numberOfPages = pagedBills ? getNumberOfPages(pagedBills.total, pageSize) : 0;

    const rowClick = (bill: Bill) => {
        setSelectedBill(bill);
        const account = accounts?.find(a => a.id === bill.accountId);
        setSelectedAccount(account);
        setShowDetails(true);
    };

    const handleFilterChange = (newFilter: BillFilter) => {
        setFilter(newFilter);
        setPageNumber(1);
    };

    return (
        <div className="utility-bills-tab">
            <BillFilterPanel accounts={accounts} filter={filter} onFilterChange={handleFilterChange} />

            <BillsChart utilityType={utilityType} filter={filter} />

            <UsageChart utilityType={utilityType} filter={filter} />

            {selectedAccount && (
                <BillDetails account={selectedAccount} bill={selectedBill!} show={showDetails} onHide={() => setShowDetails(false)} />
            )}

            <Table striped className="section">
                <thead>
                    <tr>
                        <th>Account</th>
                        <th>Date</th>
                        <th>Cost</th>
                        <th>Usage ({getUnit(utilityType)})</th>
                    </tr>
                </thead>
                <tbody>
                    {pagedBills?.results.map(bill => (
                        <tr key={bill.id} onClick={() => rowClick(bill)} className="clickable">
                            <td>{bill.accountName}</td>
                            <td>{format(parseISO(bill.issueDate), "dd/MM/yy")}</td>
                            <td>{formatCurrency(bill.cost)}</td>
                            <td>{bill.periods?.reduce((sum, p) => sum + p.totalUsage, 0).toLocaleString() ?? "-"}</td>
                        </tr>
                    ))}
                    {(!pagedBills || pagedBills.results.length === 0) && (
                        <tr>
                            <td colSpan={4} className="no-bills">No bills found</td>
                        </tr>
                    )}
                </tbody>
                {pagedBills && pagedBills.total > 0 && (
                    <tfoot>
                        <tr>
                            <td colSpan={3} className="page-totals">
                                Page {pageNumber} of {numberOfPages} ({pagedBills.total} bills)
                            </td>
                            <td>
                                <Pagination
                                    pageNumber={pageNumber}
                                    numberOfPages={numberOfPages}
                                    onChange={(_current, newPage) => setPageNumber(newPage)}
                                />
                            </td>
                        </tr>
                    </tfoot>
                )}
            </Table>
        </div>
    );
};

UtilityTypeBillsTab.displayName = "UtilityTypeBillsTab";
