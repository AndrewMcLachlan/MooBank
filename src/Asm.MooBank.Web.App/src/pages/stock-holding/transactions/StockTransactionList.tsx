
import React, { useEffect } from "react";
import { useDispatch, useSelector } from "react-redux";

import { Table } from "react-bootstrap";

import { StockTransactionsSlice } from "store";
import { State } from "store/state";
import { StockTransactionRow } from "./StockTransactionRow";
import { useStockTransactions } from "services";
import { getNumberOfPages, Pagination, useIdParams } from "@andrewmclachlan/mooapp";
import { StockTransactionTableHead } from "./StockTransactionTableHead";
import { useDebounce } from "use-debounce";
import { useAccount } from "components";
import { useStockHolding } from "../StockHoldingProvider";

export const StockTransactionList: React.FC<TransactionListProps> = () => {

    const stockHolding = useStockHolding();

    const { currentPage: pageNumber, pageSize, filter, sortField, sortDirection } = useSelector((state: State) => state.stockTransactions);
    const dispatch = useDispatch();
    const [debouncedFilter] = useDebounce(filter, 250);

    useEffect(() => { dispatch(StockTransactionsSlice.actions.setCurrentPage(1)) }, [debouncedFilter]);

    const transactionsQuery = useStockTransactions(stockHolding.id, debouncedFilter, pageSize, pageNumber, sortField, sortDirection);
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
                        <Pagination pageNumber={pageNumber} numberOfPages={numberOfPages} onChange={(_current, newPage) => dispatch(StockTransactionsSlice.actions.setCurrentPage(newPage))} />
                    </td>
                </tr>
            </tfoot>
        </Table>
    );
}

export interface TransactionListProps {
}
