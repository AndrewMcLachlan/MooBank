
import { LogicalAccount, TransactionAccount } from ".";
import { RecurringTransaction } from "./RecurringTransaction";

export interface CreateVirtualInstrument {
    name: string;
    description?: string;
    openingBalance: number;
}

export interface VirtualAccount extends TransactionAccount {
    parentId: string;
    recurringTransactions: RecurringTransaction[];
}

export const isVirtualAccount = (account: LogicalAccount | VirtualAccount): boolean =>
    !!(account as VirtualAccount).parentId;
