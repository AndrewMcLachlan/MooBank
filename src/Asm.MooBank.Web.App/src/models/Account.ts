import { Controller, Instrument, TopLevelAccount } from "./Instrument";

export const AccountTypes = ["None", "Transaction", "Savings", "Credit", "Mortgage", "Superannuation", "Investment", "Loan", "Broker"] as AccountType[];
export type AccountType = "None" | "Transaction" | "Savings" | "Credit" | "Mortgage" | "Superannuation" | "Investment" | "Loan" | "Broker";

export interface TransactionAccount extends Instrument {
    lastTransaction?: string;
    calculatedBalance: number;
}
export interface InstitutionAccount extends TransactionAccount, TopLevelAccount {
    balanceDate: Date;
    accountType: AccountType;
    importerTypeId?: number;
    isPrimary?: boolean;
    includeInBudget: boolean;
    institutionId: number;
}

export interface CreateInstitutionAccount extends InstitutionAccount {
    balance: number;
    openingDate: string;
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
