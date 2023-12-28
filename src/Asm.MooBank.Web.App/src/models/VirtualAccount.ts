
import { AccountBase, InstitutionAccount, TransactionAccount } from ".";
import { RecurringTransaction } from "./RecurringTransaction";

export interface VirtualAccount extends TransactionAccount {
    parentId: string;
    recurringTransactions: RecurringTransaction[];
}

export const isVirtualAccount = (account: InstitutionAccount | VirtualAccount): boolean =>
    !!(account as VirtualAccount).parentId;