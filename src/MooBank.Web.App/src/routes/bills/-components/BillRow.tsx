import { format, parseISO } from "date-fns";
import type { Bill, Account } from "api/types.gen";

export const BillRow: React.FC<BillRowProps> = ({ account, bill, onClick }) =>
    <tr className="clickable" onClick={() => onClick(bill)}>
        <td>{account.name}</td>
        <td>{format(parseISO(bill.issueDate), "dd/MM/yy")}</td>
        <td>{bill.cost.toFixed(2)}</td>
    </tr>
;

export interface BillRowProps {
    account: Account;
    bill: Bill;
    onClick: (bill: Bill) => void;
}
