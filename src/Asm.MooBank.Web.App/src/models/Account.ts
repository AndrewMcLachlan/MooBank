import { VirtualAccount } from "./VirtualAccount";

export const AccountTypes = ["None", "Asset", "Transaction", "Savings", "Credit", "Mortgage", "Superannuation", "Shares", "Virtual"] as AccountType[];
export type AccountType = "None" | "Asset" | "Transaction" | "Savings" | "Credit" | "Mortgage" | "Superannuation" | "Shares" | "Virtual";

export const AccountControllers = ["Manual", "Virtual", "Import"] as AccountController[];
export type AccountController = "Manual" | "Virtual" | "Import";

export type AccountId = string;

export interface AccountBase {
    id: AccountId;
    name: string;
    description?: string;
    currentBalance: number;
    currentBalanceLocalCurrency: number;
    currency: string;
}

export interface TopLevelAccount {
    shareWithFamily: boolean;
    accountGroupId: string;
}

export interface TransactionAccount extends AccountBase {
    lastTransaction?: string;
    calculatedBalance: number;
}
export interface InstitutionAccount extends TransactionAccount, TopLevelAccount {
    balanceDate: Date;
    accountType: AccountType;
    controller: AccountController;
    importerTypeId?: number;
    virtualAccountRemainingBalance?: number;
    isPrimary?: boolean;
    includeInBudget: boolean;
    institutionId: number;
    virtualAccounts: VirtualAccount[];
}

export const emptyAccount : InstitutionAccount = {
    id: "",
    name: "",
    currentBalance: 0,
    balanceDate: new Date(),
    accountType: "None",
    controller: "Manual",
    accountGroupId: "",
    calculatedBalance: 0,
    currentBalanceLocalCurrency: 0,
    currency: "",
    shareWithFamily: false,
    includeInBudget: false,
    institutionId: 0,
    virtualAccounts: [],
}

export interface ImportAccount {
    importerTypeId: number;
}
