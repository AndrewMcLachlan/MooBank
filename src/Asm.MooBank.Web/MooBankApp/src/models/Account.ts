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
    currentBalance: number;
}

export interface Account extends AccountBase {
    calculatedBalance: number;
    balanceDate: Date;
    lastTransaction?: string;
    accountType: AccountType;
    accountGroupId: string;
    controller: AccountController;
    importerTypeId?: number;
    virtualAccountRemainingBalance?: number;
    isPrimary?: boolean;
    shareWithFamily: boolean;
    institutionId: number;
    virtualAccounts: VirtualAccount[];
}

export const emptyAccount : Account = {
    id: "",
    name: "",
    currentBalance: 0,
    balanceDate: new Date(),
    accountType: AccountType.None,
    controller: AccountController.Manual,
    accountGroupId: "",
    calculatedBalance: 0,
    shareWithFamily: false,
    institutionId: 0,
    virtualAccounts: [],
}

export interface ImportAccount {
    importerTypeId: number;
}
