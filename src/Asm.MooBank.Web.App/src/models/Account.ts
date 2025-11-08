import { Controller, Instrument, TopLevelAccount } from "./Instrument";

export const AccountTypes = ["None", "Transaction", "Savings", "Credit", "Mortgage", "Superannuation", "Investment", "Loan", "Broker"] as AccountType[];
export type AccountType = "None" | "Transaction" | "Savings" | "Credit" | "Mortgage" | "Superannuation" | "Investment" | "Loan" | "Broker";

export interface TransactionAccount extends Instrument {
    lastTransaction?: string;
    calculatedBalance: number;
}
export interface LogicalAccount extends TransactionAccount, TopLevelAccount {
    balanceDate: Date;
    accountType: AccountType;
    isPrimary?: boolean;
    includeInBudget: boolean;
    institutionAccounts: InstitutionAccount[],
}

export interface CreateLogicalAccount extends LogicalAccount {
    balance: number;
    openedDate: string;
    institutionId: number;
    importerTypeId?: number;
}

export const emptyAccount: LogicalAccount = {
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
    institutionAccounts: [],
    virtualInstruments: [],
}


export interface BaseInstitutionAccount {
    institutionId: number;
    importerTypeId?: number;
    name: string;
}

export interface InstitutionAccount extends BaseInstitutionAccount {
    id: string;
    openedDate: string;
    closedDate?: string;
}

export interface CreateInstitutionAccount extends BaseInstitutionAccount {
    openedDate: string;
 }

export interface UpdateInstitutionAccount extends BaseInstitutionAccount { 

}
