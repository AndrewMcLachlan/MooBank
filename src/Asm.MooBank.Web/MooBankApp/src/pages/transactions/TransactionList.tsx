import "./TransactionList.scss";

import React from "react";
import { useDispatch, useSelector } from "react-redux";

import { Table } from "react-bootstrap";

import { TransactionsSlice } from "store/Transactions";
import { State } from "store/state";
import { TransactionRow } from "./TransactionRow";
import { TransactionRowIng } from "./TransactionRowIng";
import { useTransactions } from "services";
import { useIdParams } from "hooks";
import { getNumberOfPages } from "helpers/paging";
import { Pagination } from "components";
import { Account } from "models";
import { TransactionTableHead } from "./TransactionTableHead";
import { TransactionTableHeadIng } from "./TransactionTableHeadIng";

export const TransactionList: React.FC<TransactionListProps> = (props) => {

    const id = useIdParams();

    const { currentPage: pageNumber, pageSize, filter, sortField, sortDirection } = useSelector((state: State) => state.transactions);
    const dispatch = useDispatch();

    const transactionsQuery = useTransactions(id, filter, pageSize, pageNumber, sortField, sortDirection);
    const transactions = transactionsQuery.data?.results;
    const totalTransactions = transactionsQuery.data?.total ?? 0;

    const numberOfPages = getNumberOfPages(totalTransactions, pageSize);

    const TableHead = transactions?.some(t => t.extraInfo) ? TransactionTableHeadIng : TransactionTableHead;
    const TableRow = transactions?.some(t => t.extraInfo) ? TransactionRowIng : TransactionRow;
    const colspan = transactions?.some(t => t.extraInfo) ? 4 : 2;

    return (
        <Table striped bordered={false} borderless className="transactions">
            <TableHead />
            <tbody>
                {transactions && transactions.map((t) => <TableRow key={t.id} transaction={t} />)}
                {!transactions && Array.from({ length: 50 }, (_value, index) => index).map((i) => <tr key={i}><td colSpan={4}>&nbsp;</td></tr>)}
            </tbody>
            <tfoot>
                <tr>
                    <td colSpan={2} className="page-totals">Page {pageNumber} of {numberOfPages} ({totalTransactions} transactions)</td>
                    <td colSpan={colspan}>
                        <Pagination pageNumber={pageNumber} numberOfPages={numberOfPages} onChange={(_current, newPage) => dispatch(TransactionsSlice.actions.setCurrentPage(newPage))} />
                    </td>
                </tr>
            </tfoot>
        </Table>
    );
}

export interface TransactionListProps {
    account: Account;
}
