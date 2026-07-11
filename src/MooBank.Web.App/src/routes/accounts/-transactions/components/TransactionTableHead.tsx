import React from "react";

import { PaginationTh, SortableTh } from "@andrewmclachlan/moo-ds";
import type { PaginationProps, SortDirection } from "@andrewmclachlan/moo-ds";

import { useTransactionSearch } from "../hooks/useTransactionSearch";

export const TransactionTableHead: React.FC<TransactionTableHeadProps> = ({ compact, ...props }) => {

    const { sortField, sortDirection, setSort } = useTransactionSearch();

    const sort = (newSortField: string) => {

        let newSortDirection: SortDirection = "Ascending";

        if (newSortField === sortField) {
            newSortDirection = sortDirection === "Ascending" ? "Descending" : "Ascending";
        }

        setSort(newSortField, newSortDirection);
    }

    return (
        <thead>
            <tr className="transaction-head">
                <SortableTh hidden={compact} field="TransactionTime" sortField={sortField} sortDirection={sortDirection} onSort={sort} className="d-none d-md-table-cell">Date</SortableTh>
                <SortableTh field="Description" sortField={sortField} sortDirection={sortDirection} onSort={sort}>Description</SortableTh>
                <SortableTh hidden={compact} field="Location" sortField={sortField} sortDirection={sortDirection} onSort={sort} className="d-none d-md-table-cell">Location</SortableTh>
                <SortableTh hidden={compact} field="AccountHolderName" sortField={sortField} sortDirection={sortDirection} onSort={sort} className="d-none d-md-table-cell">Who</SortableTh>
                <SortableTh field="Amount" sortField={sortField} sortDirection={sortDirection} onSort={sort}>Amount</SortableTh>
                <PaginationTh hidden={compact} {...props} className="d-none d-md-table-cell">Tags</PaginationTh>
            </tr>
        </thead>
    );
}

export interface TransactionTableHeadProps extends PaginationProps {
    compact?: boolean;
}
