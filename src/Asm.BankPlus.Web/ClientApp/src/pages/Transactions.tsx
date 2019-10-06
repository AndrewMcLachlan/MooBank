import React, { useEffect } from "react";
//import Table from "react-bootstrap-table-next";
import { useDispatch, useSelector } from "react-redux";
import { RouteComponentProps } from "react-router";
import { bindActionCreators } from "redux";

import { Table } from "react-bootstrap";

import moment from "moment";

//import paginationFactory from 'react-bootstrap-table2-paginator';
//import 'react-bootstrap-table2-paginator/dist/react-bootstrap-table2-paginator.min.css';

import { actionCreators as accountActionCreators } from "store/Accounts";
import { actionCreators as transactionActionCreators } from "store/Transactions";
import { Accounts, Transactions as TransactionsState, State } from "store/state";
import { TransactionTag } from "../models";
import { TransactionRow } from "../components";
import { threadId } from "worker_threads";


export const Transactions: React.FC<TransactionsProps> = (props) => {

    const accountId = props.match.params.id;

    const account = useSelector((state: State) => (state.accounts.selectedAccount && state.accounts.selectedAccount.id === accountId) ? state.accounts.selectedAccount : undefined);
    const transactions = useSelector((state: State) => state.transactions.transactions);
    const pageNumber = useSelector((state: State) => state.transactions.currentPage);
    const dispatch = useDispatch();

    bindActionCreators(accountActionCreators, dispatch);
    bindActionCreators(transactionActionCreators, dispatch);

    useEffect(() => {
        let _ = (!account || account.id !== accountId) ? dispatch(accountActionCreators.requestAccount(accountId)) : null;
    }, [dispatch, account, accountId])

    useEffect(() => {
        dispatch(transactionActionCreators.requestTransactions(accountId, pageNumber));
    }, [dispatch, accountId, pageNumber])

    let dataSource = transactions;
    if (!dataSource || dataSource === null) {
        dataSource = [];
    }

    const columns = [{
        dataField: "id",
        text: "",
        hidden: true,
    }, {
        dataField: "transactionTime",
        text: "Date",
        formatter: (cell: any) => cell && moment(cell).format("DD/MM/YYYY"),
        sort: true,
    }, {
        dataField: "description",
        text: "Descripton",
        sort: true,
    }, {
        dataField: "amount",
        text: "Amount",
        formatter: (cell: number) => cell && (cell.toLocaleString("en-AU", { minimumFractionDigits: 2, maximumFractionDigits: 2 })),
        sort: true,
    }, {
        dataField: "tags",
        text: "Tags",
        formatter: (cell: TransactionTag[]) => cell.map((tt) => <p>{tt.name}</p>),

        sort: false,
    }
    ];

    return (
        <section>
            <Table striped bordered={false} borderless>
                <thead>
                    <tr>
                        <th>Date</th>
                        <th>Description</th>
                        <th>Amount</th>
                        <th>Tags</th>
                    </tr>
                </thead>
                <tbody>
                    {transactions && transactions.map((t) => <TransactionRow key={t.id} transaction={t} />) }
                </tbody>
            </Table>



            {/*            <Table keyField="id"
                data={dataSource}
                noDataIndication="No transactions to display"
                pagination={paginationFactory()}
                columns={columns}
                bordered={false}
                striped
                bootstrap4 />*/}
        </section>
    );
}

export interface TransactionsProps extends RouteComponentProps<{ id: string }> {

}