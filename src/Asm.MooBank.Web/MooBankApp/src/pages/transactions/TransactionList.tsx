import "./TransactionList.scss";

import React from "react";
import { useDispatch, useSelector } from "react-redux";

import { Table } from "react-bootstrap";

import { TransactionsSlice } from "store/Transactions";
import { sortDirection, State } from "store/state";
import { TransactionRow } from "./TransactionRow";
import { TransactionRowIng } from "./TransactionRowIng";
import { useTransactions } from "services";
import { useIdParams } from "hooks";
import { getNumberOfPages } from "helpers/paging";
import { Pagination } from "components";

export const TransactionList: React.FC<TransactionListProps> = (props) => {

    const id = useIdParams();

    const pageNumber = useSelector((state: State) => state.transactions.currentPage);
    const pageSize = useSelector((state: State) => state.transactions.pageSize);
    const filter = useSelector((state: State) => state.transactions.filter);
    const sortField = useSelector((state: State) => state.transactions.sortField);
    const sortDirection = useSelector((state: State) => state.transactions.sortDirection);
    const dispatch = useDispatch();

    const transactionsQuery = useTransactions(id, filter, pageSize, pageNumber, sortField, sortDirection);
    const transactions = transactionsQuery.data?.results;
    const totalTransactions = transactionsQuery.data?.total ?? 0;

    const numberOfPages = getNumberOfPages(totalTransactions, pageSize);

    const sort = (newSortField: string) => {

        let newSortDirection: sortDirection = "Ascending";

        if (newSortField === sortField) {
            newSortDirection = sortDirection === "Ascending" ? "Descending" : "Ascending";
        }

        dispatch(TransactionsSlice.actions.setSort([newSortField, newSortDirection]));
    }

    const getClassName = (field: string) => field === sortField ? `sortable ${sortDirection.toLowerCase()}` : `sortable`;

    return (
        <Table striped bordered={false} borderless className="transactions">
            <thead>
                <tr>
                    <th className={getClassName("TransactionTime")} onClick={() => sort("TransactionTime")}>Date</th>
                    <th className={getClassName("Description")} onClick={() => sort("Description")}>Description</th>
                    <th className={getClassName("Amount")} onClick={() => sort("Amount")}>Amount</th>
                    <th>Tags</th>
                </tr>
            </thead>
            <tbody>
                {transactions && transactions.map((t) => t.extraInfo ? <TransactionRowIng key={t.id} transaction={t} /> : <TransactionRow key={t.id} transaction={t} />)}
                {!transactions && Array.from({ length: 50 }, (value, index) => index).map((i) => <tr key={i}><td colSpan={4}>&nbsp;</td></tr>)}
            </tbody>
            <tfoot>
                <tr>
                    <td colSpan={2} className="page-totals">Page {pageNumber} of {numberOfPages} ({totalTransactions} transactions)</td>
                    <td colSpan={2}>
                        <Pagination pageNumber={pageNumber} numberOfPages={numberOfPages} onChange={(_current, newPage) => dispatch(TransactionsSlice.actions.setCurrentPage(newPage))} />
                    </td>
                </tr>
            </tfoot>
        </Table>
    );
}

export interface TransactionListProps {
}
