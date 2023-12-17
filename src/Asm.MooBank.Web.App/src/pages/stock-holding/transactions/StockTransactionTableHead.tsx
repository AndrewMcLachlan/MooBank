import { useDispatch, useSelector } from "react-redux";

import { SortDirection} from "@andrewmclachlan/mooapp";

import { State } from "store/state";
import { StockTransactionsSlice } from "store/StockTransactions";

export const StockTransactionTableHead: React.FC = () => {

    const dispatch = useDispatch();
    const { sortField, sortDirection } = useSelector((state: State) => state.transactions);

    const sort = (newSortField: string) => {

        let newSortDirection: SortDirection = "Ascending";

        if (newSortField === sortField) {
            newSortDirection = sortDirection === "Ascending" ? "Descending" : "Ascending";
        }

        dispatch(StockTransactionsSlice.actions.setSort([newSortField, newSortDirection]));
    }

    const getClassName = (field: string) => field === sortField ? `sortable ${sortDirection.toLowerCase()}` : `sortable`;

    return (
        <thead>
            <tr className="transaction-head">
                <th className={getClassName("TransactionDate")} onClick={() => sort("TransactionDate")}>Date</th>
                <th className={getClassName("Description")} onClick={() => sort("Description")}>Description</th>
                <th className={getClassName("AccountHolder")} onClick={() => sort("AccountHolder")}>Who</th>
                <th className={getClassName("Quantity")} onClick={() => sort("Quantity")}>Quantity</th>
                <th className={getClassName("Price")} onClick={() => sort("Price")}>Price</th>
                <th className={getClassName("Fees")} onClick={() => sort("Fees")}>Fees</th>
                <th className={getClassName("Total")}>Total</th>
            </tr>
        </thead>
    );
}