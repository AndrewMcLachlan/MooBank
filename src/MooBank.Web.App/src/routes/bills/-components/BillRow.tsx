import { Icon } from "@andrewmclachlan/moo-ds";
import type { Bill, Account } from "api/types.gen";
import { formatDateShort } from "utils/dateFns";
import { Amount } from "components";

export const BillRow: React.FC<BillRowProps> = ({ account, bill, onClick, onEdit }) =>
    <tr className="clickable" onClick={() => onClick(bill)}>
        <td>{account.name}</td>
        <td>{formatDateShort(bill.issueDate)}</td>
        <td><Amount amount={bill.cost} currencyCode="AUD" /></td>
        <td className="row-action column-5">
            {/* The row opens the drawer, so the edit icon has to keep its click to itself. */}
            <Icon icon="pen-to-square" title="Edit Bill" onClick={e => { e.stopPropagation(); onEdit(bill); }} />
        </td>
    </tr>
;

export interface BillRowProps {
    account: Account;
    bill: Bill;
    onClick: (bill: Bill) => void;
    onEdit: (bill: Bill) => void;
}
