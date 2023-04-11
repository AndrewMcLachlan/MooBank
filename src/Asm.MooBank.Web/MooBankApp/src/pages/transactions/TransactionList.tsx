import "./TransactionList.scss";

import React from "react";
import { useDispatch, useSelector } from "react-redux";

import { Pagination, Table } from "react-bootstrap";

import { TransactionsSlice } from "../../store/Transactions";
import { sortDirection, State } from "../../store/state";
import { TransactionRow } from "./TransactionRow";
import { TransactionRowIng } from "./TransactionRowIng";
import { useTransactions } from "../../services";
import { useIdParams } from "../../hooks";

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

    const getClassName = (field: string) => field === sortField ? `sortable ${sortDirection.toLowerCase()}` : `sortable`;

    const pagesToDisplay: number[] = [];
    if (pageNumber < 3 || numberOfPages <= 5) {
        for (let i = 1; i <= 5 && i <= numberOfPages; i++) {
            pagesToDisplay.push(i);
        }
    } else if (pageNumber >= numberOfPages - 3) {
        for (let i = numberOfPages - 4; i <= numberOfPages; i++) {
            pagesToDisplay.push(i);
        }
    } else {
        for (let i = pageNumber - 2; i <= pageNumber + 2; i++) {
            pagesToDisplay.push(i);
        }
    }

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
                        <Pagination>
                            <Pagination.First disabled={!showPrev} onClick={() => dispatch(TransactionsSlice.actions.setCurrentPage(1))} />
                            <Pagination.Prev disabled={!showPrev} onClick={() => dispatch(TransactionsSlice.actions.setCurrentPage(1))} />
                            {pagesToDisplay.map((page) => (
                                <Pagination.Item key={page} active={page === pageNumber} onClick={() => dispatch(TransactionsSlice.actions.setCurrentPage(page))}>
                                    {page}
                                </Pagination.Item>
                            ))}
                            <Pagination.Next disabled={!showNext} onClick={() => dispatch(TransactionsSlice.actions.setCurrentPage(Math.min(pageNumber + 1, numberOfPages)))} />
                            <Pagination.Last disabled={!showNext} onClick={() => dispatch(TransactionsSlice.actions.setCurrentPage(numberOfPages))} />
                        </Pagination>
                    </td>
                </tr>
            </tfoot>
        </Table>
    );
}

export interface TransactionListProps {
}
