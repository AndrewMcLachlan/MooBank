import { format, parseISO } from "date-fns";
import { Bill } from "models/bills";

export const BillRow: React.FC<BillRowProps> = ({ bill, onClick }) => {

    return (
        <>
            <tr className="clickable" onClick={() => onClick(bill)}>
                <td>{bill.accountName}</td>
                <td>{format(parseISO(bill.issueDate), "dd/MM/yy")}</td>
                <td>{bill.cost.toFixed(2)}</td>
            </tr>

        </>
    );
}

export interface BillRowProps {
    bill: Bill;
    onClick: (bill: Bill) => void;
}
