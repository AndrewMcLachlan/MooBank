import { useDispatch, useSelector } from "react-redux";
import { sortDirection, State } from "store/state";
import { TransactionsSlice } from "store/Transactions";

export const TransactionTableHeadIng: React.FC = () => {

    const dispatch = useDispatch();
    const { sortField, sortDirection } = useSelector((state: State) => state.transactions);

    const sort = (newSortField: string) => {

        let newSortDirection: sortDirection = "Ascending";

        if (newSortField === sortField) {
            newSortDirection = sortDirection === "Ascending" ? "Descending" : "Ascending";
        }

        dispatch(TransactionsSlice.actions.setSort([newSortField, newSortDirection]));
    }

    const getClassName = (field: string) => field === sortField ? `sortable ${sortDirection.toLowerCase()}` : `sortable`;

    return (
        <thead>
            <tr className="transaction-head-ing">
                <th className={getClassName("TransactionTime")} onClick={() => sort("TransactionTime")}>Date</th>
                <th className={getClassName("Description")} onClick={() => sort("Description")}>Description</th>
                {/*
                <th className={getClassName("PurchaseType")}>Purchase Type</th>
                <th className={getClassName("ReceiptNumber")}>Receipt #</th>
                */}
                <th>Location</th>
                <th>Purchase Date</th>
                <th className={getClassName("Amount")} onClick={() => sort("Amount")}>Amount</th>
                <th>Tags</th>
            </tr>
        </thead>
    );
}