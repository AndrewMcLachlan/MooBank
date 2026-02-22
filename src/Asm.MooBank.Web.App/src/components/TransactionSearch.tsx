import { ComboBox, ValueProps } from "@andrewmclachlan/moo-ds";
import { formatDate } from "utils/dateFns";
import type { Transaction, TransactionType } from "api/types.gen";
import { useSearchTransactions } from "routes/accounts/-hooks/useSearchTransactions";

export const TransactionSearch: React.FC<TransactionSearchProps> = ({ transaction, transactionType = "Credit", excludedTransactions = [], ...props }) => {

    const transactions = useSearchTransactions(transaction, transactionType);

    const filteredTransactions = (excludedTransactions && transactions.data?.filter(t => !excludedTransactions.includes(t.id))) ?? [];

    return (
        <ComboBox selectedItems={[props.value]}
            items={filteredTransactions}
            valueField={(t) => t.id}
            placeholder="Select Transaction..."
            onChange={t => props.onChange(t.length ? t[0] : null)}
            key={props.value?.id || JSON.stringify(filteredTransactions)}
            labelField={(t) => t &&
                <><span className="amount">${t.amount}</span> - {formatDate(t.transactionTime)} - {t.description}</>
            } />
    );
}

export interface TransactionSearchProps extends ValueProps<Transaction> {
    transaction: Transaction;
    transactionType?: TransactionType;
    excludedTransactions?: string[];
}
