import "./TransactionList.scss";

import React, { useEffect } from "react";
import { useDispatch, useSelector } from "react-redux";
import { bindActionCreators } from "redux";

import { Table } from "react-bootstrap";

import { actionCreators as accountActionCreators } from "../../store/Accounts";
import { actionCreators as transactionActionCreators, SetTransactionListFilter } from "../../store/Transactions";
import { State } from "../../store/state";
import { Account } from "../../models";
import { TransactionRow } from "./TransactionRow";
import { TransactionRowIng } from "./TransactionRowIng";
import { useTransactions } from "../../services";
import { useParams } from "react-router";

export const TransactionList: React.FC<TransactionListProps> = (props) => {

    const { id } = useParams<any>();

    const pageNumber = useSelector((state: State) => state.transactions.currentPage);
    const totalTransactions = useSelector((state: State) => state.transactions.total);
    const pageSize = useSelector((state: State) => state.transactions.pageSize);
    const filterTagged = useSelector((state: State) => state.transactions.filterTagged);
    const dispatch = useDispatch();

    const transactionsQuery = useTransactions(id, filterTagged, pageSize, pageNumber);
    const transactions = transactionsQuery.data?.transactions;

    bindActionCreators(accountActionCreators, dispatch);
    bindActionCreators(transactionActionCreators, dispatch);

    useEffect(() => {
        props.account && dispatch(transactionActionCreators.requestTransactions(props.account.id, pageNumber));
    }, [dispatch, props.account, filterTagged]);

    const numberOfPages = Math.ceil(totalTransactions / pageSize);
 
    const showNext = pageNumber < numberOfPages;
    const showPrev = pageNumber > 1;

    return (
        <section>
            <div className="filter-panel">
                <input id="filter-tagged" type="checkbox" onChange={(e) => dispatch({  type: SetTransactionListFilter, data: e.currentTarget.checked})} />
                <label htmlFor="filter-tagged">Only show transactions without tags</label>
            </div>
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
                            <button disabled={!showPrev} className="btn" onClick={() => dispatch(transactionActionCreators.requestTransactions(props.account.id, 1))}>&lt;&lt;</button>
                            <button disabled={!showPrev} className="btn" onClick={() => dispatch(transactionActionCreators.requestTransactions(props.account.id, Math.max(pageNumber - 1, 1)))}>&lt;</button>
                            <button disabled={!showNext} className="btn" onClick={() => dispatch(transactionActionCreators.requestTransactions(props.account.id, Math.min(pageNumber + 1, numberOfPages)))}>&gt;</button>
                            <button disabled={!showNext} className="btn" onClick={() => dispatch(transactionActionCreators.requestTransactions(props.account.id, numberOfPages))}>&gt;&gt;</button>
                        </td>
                    </tr>
                </tfoot>
            </Table>
        </section>
    );
}

export interface TransactionListProps {
    account: Account;
}