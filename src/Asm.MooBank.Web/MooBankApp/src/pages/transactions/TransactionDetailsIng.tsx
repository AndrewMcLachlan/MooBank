import format from "date-fns/format";
import parseISO from "date-fns/parseISO";
import { Transaction } from "models";

export const TransactionDetailsIng: React.FC<TransactionDetailsIngProps> = ({ transaction }) => {

    if (!transaction?.extraInfo) return null;

    return (
        <section className="transaction-details-extra">
            <div>Receipt</div>
            <div className="value">{transaction.extraInfo.receiptNumber}</div>
            <div>Purchase Type</div>
            <div className="value">{transaction.extraInfo.purchaseType}</div>
            <div>Purchase Date</div>
            <div className="value">{format(parseISO(transaction.extraInfo.purchaseDate ?? transaction.transactionTime), "dd/MM/yyyy")}</div>
            <div>Location</div>
            <div className="value">{transaction.extraInfo.location}</div>
            <div>Who</div>
            <div className="value">{transaction.extraInfo.who}</div>
            <div>Reference</div>
            <div className="value">{transaction.extraInfo.reference}</div>
        </section>
    );
}

export interface TransactionDetailsIngProps {
    transaction: Transaction;
}