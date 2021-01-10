import "./TransactionList.scss";

import React from "react";
import { useDispatch, useSelector } from "react-redux";

import { Table } from "react-bootstrap";

import { TransactionsSlice } from "../../store/Transactions";
import { State } from "../../store/state";
import { TransactionRow } from "./TransactionRow";
import { TransactionRowIng } from "./TransactionRowIng";
import { useTransactions } from "../../services";
import { useParams } from "react-router";

export const TransactionList: React.FC<TransactionListProps> = (props) => {

    const { id } = useParams<any>();

    const pageNumber = useSelector((state: State) => state.transactions.currentPage);
    const pageSize = useSelector((state: State) => state.transactions.pageSize);
    const filterTagged = useSelector((state: State) => state.transactions.filterTagged);
    const dispatch = useDispatch();

    const transactionsQuery = useTransactions(id, filterTagged, pageSize, pageNumber);
    const transactions = transactionsQuery.data?.transactions;
    const totalTransactions = transactionsQuery.data?.total ?? 0;

    const numberOfPages = Math.ceil(totalTransactions / pageSize);
  
    const showNext = pageNumber < numberOfPages;
    const showPrev = pageNumber > 1;

    return (
        <section>
            <fieldset className="filter-panel">
                <legend>Filters</legend>
                <input id="filter-tagged" type="checkbox" checked={filterTagged} onChange={(e) => dispatch(TransactionsSlice.actions.setTransactionListFilter(e.currentTarget.checked))} />
                <label htmlFor="filter-tagged">Only show transactions without tags</label>
            </fieldset>
            <Table striped bordered={false} borderless className="transactions">
                <thead>
                    <tr>
                        <th>Date</th>
                        <th>Description</th>
                        <th>Amount</th>
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
        </section>
    );
}

export interface TransactionListProps {
}
