import React from "react";
import { useDispatch, useSelector } from "react-redux";

import { SortDirection} from "@andrewmclachlan/mooapp";

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

    const getClassName = (field: string) => field === sortField ? `sortable ${sortDirection.toLowerCase()}` : `sortable`;

    return (
        <thead>
            <tr className="transaction-head">
                <th className={getClassName("TransactionTime")} onClick={() => sort("TransactionTime")}>Date</th>
                <th className={getClassName("Description")} onClick={() => sort("Description")}>Description</th>
                <th className={getClassName("Location")} onClick={() => sort("Location")}>Location</th>
                <th className={getClassName("AccountHolderName")} onClick={() => sort("AccountHolderName")}>Who</th>
                <th className={getClassName("Amount")} onClick={() => sort("Amount")}>Amount</th>
                <th>Tags</th>
            </tr>
        </thead>
    );
}