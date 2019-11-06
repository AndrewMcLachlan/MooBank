import "./TransactionList.scss";

import React, { useEffect } from "react";
//import Table from "react-bootstrap-table-next";
import { useDispatch, useSelector } from "react-redux";
import { bindActionCreators } from "redux";

import { Table } from "react-bootstrap";

//import paginationFactory from 'react-bootstrap-table2-paginator';
//import 'react-bootstrap-table2-paginator/dist/react-bootstrap-table2-paginator.min.css';

import { actionCreators as accountActionCreators } from "../../store/Accounts";
import { actionCreators as transactionActionCreators } from "../../store/Transactions";
import { State } from "../../store/state";
import { Account } from "../../models";
import { TransactionRow } from "./TransactionRow";

export const TransactionList: React.FC<TransactionListProps> = (props) => {

    const transactions = useSelector((state: State) => state.transactions.transactions);
    const pageNumber = useSelector((state: State) => state.transactions.currentPage);
    const totalTransactions = useSelector((state: State) => state.transactions.total);
    const pageSize = useSelector((state: State) => state.transactions.pageSize);
    const dispatch = useDispatch();

    bindActionCreators(accountActionCreators, dispatch);
    bindActionCreators(transactionActionCreators, dispatch);

    useEffect(() => {
        props.account && dispatch(transactionActionCreators.requestTransactions(props.account.id, pageNumber));
    }, [dispatch, props.account, pageNumber]);

    let dataSource = transactions;
    if (!dataSource || dataSource === null) {
        dataSource = [];
    }

    /*const columns = [{
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
    ];*/

    const numberOfPages = Math.ceil(totalTransactions / pageSize);

    const showNext = pageNumber < numberOfPages;
    const showPrev = pageNumber > 1;

    return (
        <section>
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
                    {transactions && transactions.map((t) => <TransactionRow key={t.id} transaction={t} />) }
                </tbody>
                <tfoot>
<tr>
    <td>Page {pageNumber} of {numberOfPages} ({totalTransactions} transactions)</td>
    <td>
    { showPrev && <button onClick={() => dispatch(transactionActionCreators.requestTransactions(props.account.id, Math.max(pageNumber - 1,1)))}>Prev</button>}
    { showNext && <button onClick={() => dispatch(transactionActionCreators.requestTransactions(props.account.id, Math.min(pageNumber + 1,numberOfPages)))}>Next</button>}
    </td>
</tr>
                </tfoot>
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

export interface TransactionListProps {
    account: Account;
}