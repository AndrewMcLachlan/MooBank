import { ValueProps } from "@andrewmclachlan/mooapp";
import { Transaction, TransactionTag, TransactionType } from "models";
import Select from "react-select";
import { useSearchTransactions } from "services";

export const TransactionSearch: React.FC<TransactionSearchProps> = (props) => {

    const transactions = useSearchTransactions(props.transaction, props.transactionType);

    return (
        <Select value={props.value} options={transactions.data ?? []} getOptionValue={(t) => t.id} getOptionLabel={(t) => `${t.amount} - ${t.description}`} isClearable className="react-select" classNamePrefix="react-select" placeholder="Select Transaction..." onChange={props.onChange} />
    );
}

TransactionSearch.defaultProps = {
    transactionType: TransactionType.Credit,
};

export interface TransactionSearchProps extends ValueProps<Transaction> {
    transaction: Transaction;
    transactionType?: TransactionType;
}