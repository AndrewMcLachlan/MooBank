
import React from "react";

import { Table } from "@andrewmclachlan/moo-ds";

import { getNumberOfPages, Pagination } from "@andrewmclachlan/moo-ds";
import { useStockTransactions } from "routes/shares/-hooks/useStockTransactions";
import { useStockHolding } from "../../../-components/StockHoldingProvider";
import { useStockTransactionSearch } from "../-hooks/useStockTransactionSearch";
import { StockTransactionRow } from "./StockTransactionRow";
import { StockTransactionTableHead } from "./StockTransactionTableHead";

export const StockTransactionList: React.FC<TransactionListProps> = () => {

    const stockHolding = useStockHolding();

    const { filter, page: pageNumber, pageSize, sortField, sortDirection, setPage } = useStockTransactionSearch();

    const transactionsQuery = useStockTransactions(stockHolding.id, filter, pageSize, pageNumber, sortField, sortDirection);
    const transactions = transactionsQuery.data?.results;
    const totalTransactions = transactionsQuery.data?.total ?? 0;

    const numberOfPages = getNumberOfPages(totalTransactions, pageSize);

    return (
        <Table striped bordered={false} borderless className="transactions">
            <StockTransactionTableHead />
            <tbody>
                {transactions && transactions.map((t) => <StockTransactionRow key={t.id} transaction={t} />)}
                {!transactions && Array.from({ length: 50 }, (_value, index) => index).map((i) => <tr key={i}><td colSpan={7}>&nbsp;</td></tr>)}
            </tbody>
            <tfoot>
                <tr>
                    <td colSpan={2} className="page-totals">Page {pageNumber} of {numberOfPages} ({totalTransactions} transactions)</td>
                    <td colSpan={5}>
                        <Pagination pageNumber={pageNumber} numberOfPages={numberOfPages} onChange={(_current, newPage) => setPage(newPage)} />
                    </td>
                </tr>
            </tfoot>
        </Table>
    );
}

export interface TransactionListProps {
}
