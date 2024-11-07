import React, { useEffect, useState } from "react";
import { Table } from "react-bootstrap";
import { useDispatch, useSelector } from "react-redux";

import { getNumberOfPages, Pagination } from "@andrewmclachlan/mooapp";
import { useAccount } from "components";
import { Transaction, TransactionOffset, TransactionSplit } from "models";
import { useTransactions, useUpdateTransaction } from "services";
import { State } from "store/state";
import { TransactionsSlice } from "store/Transactions";
import { useDebounce } from "use-debounce";
import { TransactionDetails } from "./details/TransactionDetails";
import { TransactionRow } from "./TransactionRow";
import { TransactionTableHead } from "./TransactionTableHead";

export const TransactionList: React.FC = () => {

    const account = useAccount();

    const { currentPage: pageNumber, pageSize, filter, sortField, sortDirection } = useSelector((state: State) => state.transactions);
    const dispatch = useDispatch();
    const [debouncedFilter] = useDebounce(filter, 250);
    const [showDetails, setShowDetails] = useState(false);
    const [selectedTransaction, setSelectedTransaction] = useState<Transaction>(undefined);

    useEffect(() => { dispatch(TransactionsSlice.actions.setCurrentPage(1)) }, [debouncedFilter]);

    const transactionsQuery = useTransactions(account.id, debouncedFilter, pageSize, pageNumber, sortField, sortDirection);
    const transactions = transactionsQuery.data?.results;
    const totalTransactions = transactionsQuery.data?.total ?? 0;

    const numberOfPages = getNumberOfPages(totalTransactions, pageSize);

    const updateTransaction = useUpdateTransaction();

    const onSave = (excludeFromReporting: boolean, notes?: string, splits?: TransactionSplit[], _offsetBy?: TransactionOffset[]) => {

        updateTransaction(selectedTransaction.accountId, selectedTransaction.id, { excludeFromReporting, notes, splits });
        setShowDetails(false);
    }

    const rowClick = (transaction: Transaction) => () => {
        setSelectedTransaction(transaction);
        setShowDetails(true);
    }

    return (
        <>
            <TransactionDetails transaction={selectedTransaction} show={showDetails} onHide={() => setShowDetails(false)} onSave={onSave} />
            <Table striped bordered={false} borderless className="transactions">
                <TransactionTableHead />
                <tbody>
                    {transactions && transactions.map((t) => <TransactionRow key={t.id} transaction={t} onClick={rowClick(t)} />)}
                    {!transactions && Array.from({ length: 50 }, (_value, index) => index).map((i) => <tr key={i}><td colSpan={6}>&nbsp;</td></tr>)}
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
        </>
    );
}
