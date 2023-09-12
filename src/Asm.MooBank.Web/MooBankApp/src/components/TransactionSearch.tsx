import { ValueProps } from "@andrewmclachlan/mooapp";
import { Transaction, Tag, TransactionType } from "models";
import Select from "react-select";
import { useSearchTransactions } from "services";

export const TransactionSearch: React.FC<TransactionSearchProps> = (props) => {

    const transactions = useSearchTransactions(props.transaction, props.transactionType);

    const filteredTransactions = (props.excludedTransactions && transactions.data?.filter(t => !props.excludedTransactions.includes(t.id))) ?? [];
    if (!props.value) 
    {
        console.debug("-");
        console.debug(props.excludedTransactions);
    console.debug(filteredTransactions);
    }

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
                    <><span className="amount">${t.amount}</span> - {t.description}</>
                } />
    );
}

TransactionSearch.defaultProps = {
    transactionType: TransactionType.Credit,
    excludedTransactions: [],
};

export interface TransactionSearchProps extends ValueProps<Transaction> {
    transaction: Transaction;
    transactionType?: TransactionType;
    excludedTransactions?: string[];
}