import { LogicalAccount, TransactionAccount } from ".";
import { Controller } from "./Instrument";
import { RecurringTransaction } from "./RecurringTransaction";

export interface CreateVirtualInstrument {
    name: string;
    description?: string;
    openingBalance: number;
    controller: Controller;
}

export interface VirtualInstrument extends TransactionAccount {
    parentId: string;
    recurringTransactions: RecurringTransaction[];
    closedDate?: string;
}

export interface UpdateVirtualInstrument
{
    name?: string;
    description?: string;
}

export const isVirtualInstrument = (account: LogicalAccount | VirtualInstrument): boolean =>
    !!(account as VirtualInstrument).parentId;
