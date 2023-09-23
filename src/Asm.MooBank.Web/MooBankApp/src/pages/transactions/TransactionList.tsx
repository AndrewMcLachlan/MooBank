import "./TransactionList.scss";

import React, { useEffect } from "react";
import { useDispatch, useSelector } from "react-redux";

import { Table } from "react-bootstrap";

import { TransactionsSlice } from "store/Transactions";
import { State } from "store/state";
import { TransactionRow } from "./TransactionRow";
import { useTransactions } from "services";
import { getNumberOfPages, Pagination, useIdParams } from "@andrewmclachlan/mooapp";
import { Account } from "models";
import { TransactionTableHead } from "./TransactionTableHead";
import { useDebounce } from "use-debounce";

export const TransactionList: React.FC<TransactionListProps> = (props) => {

    const { currentPage: pageNumber, pageSize, filter, sortField, sortDirection } = useSelector((state: State) => state.transactions);
    const dispatch = useDispatch();
    const [debouncedFilter] = useDebounce(filter, 250);

    useEffect(() => { dispatch(TransactionsSlice.actions.setCurrentPage(1)) }, [debouncedFilter]);

    const transactionsQuery = useTransactions(props.account.id, debouncedFilter, pageSize, pageNumber, sortField, sortDirection);
    const transactions = transactionsQuery.data?.results;
    const totalTransactions = transactionsQuery.data?.total ?? 0;

    const numberOfPages = getNumberOfPages(totalTransactions, pageSize);

    return (
        <Table striped bordered={false} borderless className="transactions">
            <TransactionTableHead />
            <tbody>
                {transactions && transactions.map((t) => <TransactionRow key={t.id} transaction={t} />)}
                {!transactions && Array.from({ length: 50 }, (_value, index) => index).map((i) => <tr key={i}><td colSpan={4}>&nbsp;</td></tr>)}
            </tbody>
            <tfoot>
                <tr>
                    <td colSpan={2} className="page-totals">Page {pageNumber} of {numberOfPages} ({totalTransactions} transactions)</td>
                    <td colSpan={4}>
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
