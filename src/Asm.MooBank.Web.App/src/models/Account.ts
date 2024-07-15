import { VirtualAccount } from "./VirtualAccount";

export const AccountTypes = ["None", "Transaction", "Savings", "Credit", "Mortgage", "Superannuation", "Investment", "Loan", "Broker"] as AccountType[];
export type AccountType = "None" | "Transaction" | "Savings" | "Credit" | "Mortgage" | "Superannuation" | "Investment" | "Loan" | "Broker";

export const Controllers = ["Manual", "Virtual", "Import"] as Controller[];
export type Controller = "Manual" | "Virtual" | "Import";

export type InstrumentId = string;

export interface Instrument {
    id: InstrumentId;
    name: string;
    controller: Controller;
    description?: string;
    currentBalance: number;
    currentBalanceLocalCurrency: number;
    currency: string;
    instrumentType?: string;
    virtualInstruments: VirtualAccount[];
}

export interface TopLevelAccount {
    shareWithFamily: boolean;
    groupId: string;
}

export interface TransactionAccount extends Instrument {
    lastTransaction?: string;
    calculatedBalance: number;
}
export interface InstitutionAccount extends TransactionAccount, TopLevelAccount {
    balanceDate: Date;
    accountType: AccountType;
    importerTypeId?: number;
    virtualAccountRemainingBalance?: number;
    isPrimary?: boolean;
    includeInBudget: boolean;
    institutionId: number;
}

export interface CreateInstitutionAccount {
    name: string;
    description?: string;
    institutionId: number;
    currency: string;
    balance: number;
    groupId: string;
    accountType: AccountType;
    importerTypeId?: number;
    controller: Controller;
    includeInBudget: boolean;
    shareWithFamily: boolean;
}

export const emptyAccount : InstitutionAccount = {
    id: "",
    name: "",
    currentBalance: 0,
    balanceDate: new Date(),
    accountType: "None",
    controller: "Manual",
    groupId: "",
    calculatedBalance: 0,
    currentBalanceLocalCurrency: 0,
    currency: "",
    shareWithFamily: false,
    includeInBudget: false,
    institutionId: 0,
    virtualInstruments: [],
}

export interface ImportAccount {
    importerTypeId: number;
}
