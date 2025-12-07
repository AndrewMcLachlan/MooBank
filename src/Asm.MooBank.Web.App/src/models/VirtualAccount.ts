import { LogicalAccount, TransactionAccount } from ".";
import { Controller } from "./Instrument";
import { RecurringTransaction } from "./RecurringTransaction";

export interface CreateVirtualInstrument {
    name: string;
    description?: string;
    openingBalance: number;
    controller: Controller;
}

export interface VirtualAccount extends TransactionAccount {
    parentId: string;
    recurringTransactions: RecurringTransaction[];
    closedDate?: string;
}

export const isVirtualAccount = (account: LogicalAccount | VirtualAccount): boolean =>
    !!(account as VirtualAccount).parentId;
