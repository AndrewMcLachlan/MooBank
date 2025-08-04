import React from "react";
import { useDispatch, useSelector } from "react-redux";

import { PaginationProps, PaginationTh, SortableTh, SortDirection } from "@andrewmclachlan/mooapp";

import { State } from "store/state";
import { TransactionsSlice } from "store/Transactions";

export const TransactionTableHead: React.FC<TransactionTableHeadProps> = ({ compact, ...props }) => {

    const dispatch = useDispatch();
    const { sortField, sortDirection } = useSelector((state: State) => state.transactions);

    const sort = (newSortField: string) => {

        let newSortDirection: SortDirection = "Ascending";

        if (newSortField === sortField) {
            newSortDirection = sortDirection === "Ascending" ? "Descending" : "Ascending";
        }

        dispatch(TransactionsSlice.actions.setSort([newSortField, newSortDirection]));
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
