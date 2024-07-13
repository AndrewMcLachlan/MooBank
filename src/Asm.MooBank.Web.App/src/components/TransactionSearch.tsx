import { ValueProps } from "@andrewmclachlan/mooapp";
import { formatDate } from "helpers/dateFns";
import { Transaction, TransactionType } from "models";
import Select from "react-select";
import { useSearchTransactions } from "services";

export const TransactionSearch: React.FC<TransactionSearchProps> = ({ transaction, transactionType = "Credit", excludedTransactions = [], ...props }) => {

    const transactions = useSearchTransactions(transaction, transactionType);

    const filteredTransactions = (excludedTransactions && transactions.data?.filter(t => !excludedTransactions.includes(t.id))) ?? [];

    return (
        <Select value={props.value}
            options={filteredTransactions}
            getOptionValue={(t) => t.id}
            className="react-select"
            classNamePrefix="react-select"
            placeholder="Select Transaction..."
            onChange={props.onChange}
            key={props.value?.id || JSON.stringify(filteredTransactions)}
            formatOptionLabel={(t) =>
                <><span className="amount">${t.amount}</span> - {formatDate(t.transactionTime)} - {t.description}</>
            } />
    );
}

export interface TransactionSearchProps extends ValueProps<Transaction> {
    transaction: Transaction;
    transactionType?: TransactionType;
    excludedTransactions?: string[];
}
