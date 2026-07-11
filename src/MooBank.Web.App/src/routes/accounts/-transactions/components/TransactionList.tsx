import React, { useState } from "react";

import { getNumberOfPages, Pagination, PaginationControls, PageSize, SectionTable, MiniPagination } from "@andrewmclachlan/moo-ds";

import { useAccount } from "components";
import type { Transaction } from "api/types.gen";
import { useTransactions } from "routes/accounts/-hooks/useTransactions";
import { useTransactionSearch } from "../hooks/useTransactionSearch";
import { TransactionDetails } from "../details/TransactionDetails";
import { TransactionRow } from "./TransactionRow";
import { TransactionTableHead } from "./TransactionTableHead";
import { parseISO } from "date-fns";

export const TransactionList: React.FC<TransactionListProps> = ({compact = false}) => {

    const account = useAccount();

    const { debouncedFilter, page: pageNumber, pageSize, setPageSize, sortField, sortDirection, setPage } = useTransactionSearch();
    const [showDetails, setShowDetails] = useState(false);
    const [selectedTransaction, setSelectedTransaction] = useState<Transaction>(undefined);

    const transactionsQuery = useTransactions(account.id, debouncedFilter, pageSize, pageNumber, sortField, sortDirection);
    const transactions = transactionsQuery.data?.results;
    const totalTransactions = transactionsQuery.data?.total ?? 0;

    const numberOfPages = getNumberOfPages(totalTransactions, pageSize);

    const rowClick = (transaction: Transaction) => {
        setSelectedTransaction(transaction);
        setShowDetails(true);
    };

    const PaginationControl = compact ? MiniPagination : Pagination;

    const className = compact ? "transactions-mobile d-table d-md-none" : "transactions d-none d-md-table";

    return (
        <>
            <TransactionDetails key={selectedTransaction?.id} transaction={selectedTransaction} show={showDetails} onHide={() => setShowDetails(false)} onSave={() => setShowDetails(false)} />
            <SectionTable className={className}>
                <TransactionTableHead compact={compact} pageNumber={pageNumber} numberOfPages={numberOfPages} onChange={(_current, newPage) => setPage(newPage)} />
                <tbody>
                    {transactions?.map((t, index) =>
                        <TransactionRow compact={compact} key={t.id} transaction={t} onClick={rowClick} previousDate={index > 0 ? parseISO(transactions[index - 1].transactionTime) : undefined} />
                    )}
                    {!transactions && Array.from({ length: pageSize }, (_value, index) => index).map((i) => <tr key={i}><td colSpan={6}>&nbsp;</td></tr>)}
                </tbody>
                <tfoot>
                    <tr>
                        <td hidden={compact} colSpan={2} className="page-totals">Page {pageNumber} of {numberOfPages} ({totalTransactions} transactions)</td>
                        <td colSpan={compact ? 2 : 4}>
                            <PaginationControls>
                                <PageSize value={pageSize} onChange={(newPageSize) => setPageSize(newPageSize)} />
                                <PaginationControl pageNumber={pageNumber} numberOfPages={numberOfPages} onChange={(_current, newPage) => setPage(newPage)} />
                            </PaginationControls>
                        </td>

                    </tr>
                </tfoot>
            </SectionTable>
        </>
    );
}

export interface TransactionListProps {
    compact?: boolean;
}
