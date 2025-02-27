import { format, parseISO } from "date-fns";
import { Bill, BillAccount } from "models/bills";

export const BillRow: React.FC<BillRowProps> = ({ account, bill, onClick }) =>
    <tr className="clickable" onClick={() => onClick(bill)}>
        <td>{account.name}</td>
        <td>{format(parseISO(bill.issueDate), "dd/MM/yy")}</td>
        <td>{bill.cost.toFixed(2)}</td>
    </tr>
;

export interface BillRowProps {
    account: BillAccount;
    bill: Bill;
    onClick: (bill: Bill) => void;
}
