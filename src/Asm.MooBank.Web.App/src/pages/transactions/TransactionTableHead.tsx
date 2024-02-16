import React from "react";
import { useDispatch, useSelector } from "react-redux";

import { SortableTh, SortDirection } from "@andrewmclachlan/mooapp";

import { State } from "store/state";
import { TransactionsSlice } from "store/Transactions";

export const TransactionTableHead: React.FC = () => {

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
                <SortableTh field="TransactionTime" sortField={sortField} sortDirection={sortDirection} onSort={sort}>Date</SortableTh>
                <SortableTh field="Description" sortField={sortField} sortDirection={sortDirection} onSort={sort}>Description</SortableTh>
                <SortableTh field="Location" sortField={sortField} sortDirection={sortDirection} onSort={sort} className="d-none d-md-table-cell">Location</SortableTh>
                <SortableTh field="AccountHolderName" sortField={sortField} sortDirection={sortDirection} onSort={sort} className="d-none d-md-table-cell">Who</SortableTh>
                <SortableTh field="Amount" sortField={sortField} sortDirection={sortDirection} onSort={sort}>Amount</SortableTh>
                <th>Tags</th>
            </tr>
        </thead>
    );
}