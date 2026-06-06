import type { Bill, Account } from "api/types.gen";
import { formatDateShort } from "utils/dateFns";
import { Amount } from "components";

export const BillRow: React.FC<BillRowProps> = ({ account, bill, onClick }) =>
    <tr className="clickable" onClick={() => onClick(bill)}>
        <td>{account.name}</td>
        <td>{formatDateShort(bill.issueDate)}</td>
        <td><Amount amount={bill.cost} currencyCode="AUD" /></td>
    </tr>
;

export interface BillRowProps {
    account: Account;
    bill: Bill;
    onClick: (bill: Bill) => void;
}
