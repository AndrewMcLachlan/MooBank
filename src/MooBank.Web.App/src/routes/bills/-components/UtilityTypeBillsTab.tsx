import React, { useMemo, useState } from "react";
import { Icon, Table } from "@andrewmclachlan/moo-ds";
import { format, subYears } from "date-fns";
import { getNumberOfPages, Pagination, useLocalStorage } from "@andrewmclachlan/moo-ds";

import type { Bill, UtilityType, Account } from "api/types.gen";
import type { BillFilter } from "../-hooks/types";
import { useBillsByUtilityType } from "../-hooks/useBillsByUtilityType";
import { useBillAccountsByType } from "../-hooks/useBillAccountsByType";
import { BillDetails } from "./BillDetails";
import { EditBill } from "./EditBill";
import { BillFilterPanel } from "./BillFilterPanel";
import { BillsChart } from "./BillsChart";
import { UsageChart } from "./UsageChart";
import { getUnit } from "utils/units";
import { billPeriodOptions } from "../-utils/billPeriodOptions";
import { formatDateShort } from "utils/dateFns";
import { Amount } from "components";

export interface UtilityTypeBillsTabProps {
    utilityType: UtilityType;
}

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
    const [selectedAccount, setSelectedAccount] = useState<Account | undefined>(undefined);
    const [editingBill, setEditingBill] = useState<Bill | undefined>(undefined);
    const [editingAccount, setEditingAccount] = useState<Account | undefined>(undefined);

    const { data: accounts } = useBillAccountsByType(utilityType);
    const { data: pagedBills } = useBillsByUtilityType(utilityType, pageNumber, pageSize, filter);

    const numberOfPages = pagedBills ? getNumberOfPages(pagedBills.total, pageSize) : 0;

    // Derived from the bills on show, so "Last period" is a period this account was actually billed
    // for. A filter narrow enough to return none leaves only the calendar entries.
    const presets = useMemo(() => billPeriodOptions(pagedBills?.results), [pagedBills?.results]);

    const editBill = (bill: Bill) => {
        setEditingBill(bill);
        setEditingAccount(accounts?.find(a => a.id === bill.accountId));
    };

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
            <BillFilterPanel accounts={accounts} filter={filter} onFilterChange={handleFilterChange} presets={presets} />

            <BillsChart utilityType={utilityType} filter={filter} />

            <UsageChart utilityType={utilityType} filter={filter} />

            {selectedAccount && (
                <BillDetails account={selectedAccount} bill={selectedBill!} show={showDetails} onHide={() => setShowDetails(false)} />
            )}

            {editingBill && editingAccount && (
                <EditBill accountId={editingAccount.id} bill={editingBill} show onHide={() => setEditingBill(undefined)} />
            )}

            <Table striped className="section">
                <thead>
                    <tr>
                        <th>Account</th>
                        <th>Date</th>
                        <th>Cost</th>
                        <th>Usage ({getUnit(utilityType)})</th>
                        <th className="row-action column-5"></th>
                    </tr>
                </thead>
                <tbody>
                    {pagedBills?.results.map(bill => (
                        <tr key={bill.id} onClick={() => rowClick(bill)} className="clickable">
                            <td>{bill.accountName}</td>
                            <td>{formatDateShort(bill.issueDate)}</td>
                            <td><Amount amount={bill.cost} currencyCode="AUD" /></td>
                            <td>{bill.periods?.reduce((sum, p) =>
                                sum + p.usages.filter(u => u.usageType === "Consumption").reduce((units, u) => units + u.totalUsage, 0), 0).toLocaleString() ?? "-"}</td>
                            <td className="row-action column-5">
                                {/* The row opens the drawer, so the edit icon has to keep its click to itself. */}
                                <Icon icon="pen-to-square" title="Edit Bill" onClick={e => { e.stopPropagation(); editBill(bill); }} />
                            </td>
                        </tr>
                    ))}
                    {(!pagedBills || pagedBills.results.length === 0) && (
                        <tr>
                            <td colSpan={5} className="no-bills">No bills found</td>
                        </tr>
                    )}
                </tbody>
                {pagedBills && pagedBills.total > 0 && (
                    <tfoot>
                        <tr>
                            <td colSpan={4} className="page-totals">
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
