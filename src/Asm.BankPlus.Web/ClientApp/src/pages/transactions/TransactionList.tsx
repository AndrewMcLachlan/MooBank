import "./TransactionList.scss";

import React from "react";
import { useDispatch, useSelector } from "react-redux";

import { Table } from "react-bootstrap";

import { TransactionsSlice } from "../../store/Transactions";
import { sortDirection, State } from "../../store/state";
import { TransactionRow } from "./TransactionRow";
import { TransactionRowIng } from "./TransactionRowIng";
import { useTransactions } from "../../services";
import { useParams } from "react-router";

export const TransactionList: React.FC<TransactionListProps> = (props) => {

    const { id } = useParams<any>();

    const pageNumber = useSelector((state: State) => state.transactions.currentPage);
    const pageSize = useSelector((state: State) => state.transactions.pageSize);
    const filter = useSelector((state: State) => state.transactions.filter);
    const sortField = useSelector((state: State) => state.transactions.sortField);
    const sortDirection = useSelector((state: State) => state.transactions.sortDirection);
    const dispatch = useDispatch();

    const transactionsQuery = useTransactions(id, filter, pageSize, pageNumber, sortField, sortDirection);
    const transactions = transactionsQuery.data?.transactions;
    const totalTransactions = transactionsQuery.data?.total ?? 0;

    const numberOfPages = Math.ceil(totalTransactions / pageSize);

    const showNext = pageNumber < numberOfPages;
    const showPrev = pageNumber > 1;

    const sort = (newSortField: string) => {

        let newSortDirection: sortDirection = "Ascending";

        if (newSortField === sortField) {
            newSortDirection = sortDirection === "Ascending" ? "Descending" : "Ascending";
        }

        dispatch(TransactionsSlice.actions.setSort([newSortField, newSortDirection]));
    }

    const getClassName = (field: string) =>
        field === sortField ? `sortable ${sortDirection.toLowerCase()}` : `sortable`;

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
            </tbody>
            <tfoot>
                <tr>
                    <td colSpan={2}>Page {pageNumber} of {numberOfPages} ({totalTransactions} transactions)</td>
                    <td colSpan={2}>
                        <button disabled={!showPrev} className="btn" onClick={() => dispatch(TransactionsSlice.actions.setCurrentPage(1))}>&lt;&lt;</button>
                        <button disabled={!showPrev} className="btn" onClick={() => dispatch(TransactionsSlice.actions.setCurrentPage(Math.max(pageNumber - 1, 1)))}>&lt;</button>
                        <button disabled={!showNext} className="btn" onClick={() => dispatch(TransactionsSlice.actions.setCurrentPage(Math.min(pageNumber + 1, numberOfPages)))}>&gt;</button>
                        <button disabled={!showNext} className="btn" onClick={() => dispatch(TransactionsSlice.actions.setCurrentPage(numberOfPages))}>&gt;&gt;</button>
                    </td>
                </tr>
            </tfoot>
        </Table>
    );
}

export interface TransactionListProps {
}
