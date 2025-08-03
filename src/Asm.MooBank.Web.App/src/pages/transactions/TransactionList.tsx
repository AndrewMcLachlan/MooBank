import React, { useEffect, useState } from "react";
import { useDispatch, useSelector } from "react-redux";

import { getNumberOfPages, Pagination, PaginationControls, PageSize, SectionTable, MiniPagination } from "@andrewmclachlan/mooapp";

import { useAccount } from "components";
import { Transaction } from "models";
import { useTransactions } from "services";
import { State } from "store/state";
import { TransactionsSlice } from "store/Transactions";
import { useDebounce } from "use-debounce";
import { TransactionDetails } from "./details/TransactionDetails";
import { TransactionRow } from "./TransactionRow";
import { TransactionTableHead } from "./TransactionTableHead";
import { parseISO } from "date-fns";

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

    const rowClick = (transaction: Transaction) => () => {
        setSelectedTransaction(transaction);
        setShowDetails(true);
    };

    return (
        <>
            <TransactionDetails transaction={selectedTransaction} show={showDetails} onHide={() => setShowDetails(false)} onSave={() => setShowDetails(false)} />
            <SectionTable className="transactions">
                <TransactionTableHead pageNumber={pageNumber} numberOfPages={numberOfPages} onChange={(_current, newPage) => dispatch(TransactionsSlice.actions.setCurrentPage(newPage))} />
                <tbody>
                    {transactions?.map((t, index) =>
                        <TransactionRow key={t.id} transaction={t} onClick={rowClick(t)} previousDate={index > 0 ? parseISO(transactions[index - 1].transactionTime) : undefined} />
                    )}
                    {!transactions && Array.from({ length: pageSize }, (_value, index) => index).map((i) => <tr key={i}><td colSpan={6}>&nbsp;</td></tr>)}
                </tbody>
                <tfoot>
                    <tr>
                        <td colSpan={2} className="page-totals d-none d-md-table-cell">Page {pageNumber} of {numberOfPages} ({totalTransactions} transactions)</td>
                        <td colSpan={4} className="d-none d-md-table-cell">
                            <PaginationControls>
                                <PageSize value={pageSize} onChange={(newPageSize) => dispatch(TransactionsSlice.actions.setPageSize(newPageSize))} />
                                <Pagination pageNumber={pageNumber} numberOfPages={numberOfPages} onChange={(_current, newPage) => dispatch(TransactionsSlice.actions.setCurrentPage(newPage))} />
                            </PaginationControls>
                        </td>
                        <td colSpan={2} className="d-md-none d-table-cell">
                            <PaginationControls>
                                <PageSize value={pageSize} onChange={(newPageSize) => dispatch(TransactionsSlice.actions.setPageSize(newPageSize))} />
                                <MiniPagination
                                    pageNumber={pageNumber}
                                    numberOfPages={numberOfPages}
                                    onChange={(_current, newPage) => dispatch(TransactionsSlice.actions.setCurrentPage(newPage))} />
                            </PaginationControls>
                        </td>
                    </tr>
                </tfoot>
            </SectionTable>
        </>
    );
}
