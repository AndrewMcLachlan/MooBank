import { VirtualAccount } from "./VirtualAccount";

export const AccountTypes = ["None", "Transaction", "Savings", "Credit", "Mortgage", "Superannuation"] as AccountType[];
export type AccountType = "None" | "Transaction" | "Savings" | "Credit" | "Mortgage" | "Superannuation" | "Stock Holding" | "Virtual";

export const AccountControllers = ["Manual", "Virtual", "Import"] as AccountController[];
export type AccountController = "Manual" | "Virtual" | "Import";

export type accountId = string;

export interface AccountBase {
    id: accountId;
    name: string;
    description?: string;
    accountGroupId: string;
    currentBalance: number;
    shareWithFamily: boolean;
}

export interface TransactionAccount extends AccountBase {
    lastTransaction?: string;
    calculatedBalance: number;
}
export interface Account extends TransactionAccount {
    balanceDate: Date;
    accountType: AccountType;
    controller: AccountController;
    importerTypeId?: number;
    virtualAccountRemainingBalance?: number;
    isPrimary?: boolean;
    institutionId: number;
    virtualAccounts: VirtualAccount[];
}

export const emptyAccount : Account = {
    id: "",
    name: "",
    currentBalance: 0,
    balanceDate: new Date(),
    accountType: "None",
    controller: "Manual",
    accountGroupId: "",
    calculatedBalance: 0,
    shareWithFamily: false,
    institutionId: 0,
    virtualAccounts: [],
}

export interface ImportAccount {
    importerTypeId: number;
}
