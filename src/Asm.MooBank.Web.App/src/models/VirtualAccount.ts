
import { AccountBase, InstitutionAccount, TransactionAccount } from ".";

export interface VirtualAccount extends TransactionAccount {
    parentId: string;
}

export const isVirtualAccount = (account: InstitutionAccount | VirtualAccount): boolean =>
    !!(account as VirtualAccount).parentId;