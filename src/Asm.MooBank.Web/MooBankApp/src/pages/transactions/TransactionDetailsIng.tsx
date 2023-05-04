import format from "date-fns/format";
import parseISO from "date-fns/parseISO";
import { Transaction } from "models";

export const TransactionDetailsIng: React.FC<TransactionDetailsIngProps> = ({ transaction }) => {

    if (!transaction?.extraInfo) return null;

    return (
        <>
            <div>Receipt</div>
            <div className="value">{transaction.extraInfo.receiptNumber}</div>
            <div>Purchase Type</div>
            <div className="value">{transaction.extraInfo.purchaseType}</div>
            <div>Purchase Date</div>
            <div className="value">{format(parseISO(transaction.extraInfo.purchaseDate ?? transaction.transactionTime), "dd/MM/yyyy")}</div>
            <div>Location</div>
            <div className="value">{transaction.extraInfo.location}</div>
        </>
    );
}

export interface TransactionDetailsIngProps {
    transaction: Transaction;
}