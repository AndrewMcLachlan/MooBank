import { VirtualAccount } from "./VirtualAccount";

export enum AccountType {
    None = 0,
    Transaction = 1,
    Savings = 2,
    Credit = 3,
    Mortgage = 4,
    Superannuation = 5,
}

export enum AccountController {
    Manual = 0,
    Virtual = 1,
    Import = 2,
}

export type accountId = string;

export interface AccountBase {
    id: accountId;
    name: string;
    description?: string;
}

export interface Account extends AccountBase {
    currentBalance: number;
    availableBalance: number;
    balanceUpdated: Date;
    accountType: AccountType;
    accountGroupId: string;
    controller: AccountController;
    importerTypeId?: number;
    virtualAccountRemainingBalance?: number;
    virtualAccounts: VirtualAccount[];
}

export const emptyAccount : Account = {
    id: "",
    name: "",
    currentBalance: 0,
    availableBalance: 0,
    balanceUpdated: new Date(),
    accountType: AccountType.None,
    controller: AccountController.Manual,
    includeInPosition: false,
    accountGroupId: "",
    virtualAccounts: [],
}

export interface ImportAccount {
    importerTypeId: number;
}
