import * as Models from "../models";

export interface State {
    app: App,
    accounts?: Accounts,
    transactions?: Transactions,
    transactionTags?: TransactionTags,
    security?: Security,
}

export interface App {
    appName: string;
    skin: string;
    baseUrl: string;
    message?: string;
}

export interface Accounts {
    accounts: Models.Account[];
    virtualAccounts: VirtualAccount[];
    areLoading: boolean;
    selectedAccount?: Models.Account;
}

export interface Transactions {
    transactions: Models.Transaction[];
    areLoading: boolean;
    currentPage: number;
}

export interface TransactionTags {
    tags: Models.TransactionTag[],
    areLoading: boolean;
}

export interface Security {
    loggedIn: boolean;
    name: string;
}

export interface VirtualAccount {
    virtualAccountId: string;
    name: string;
    description: string;
    balance: number;
    defaultAccount: boolean;
    closed: boolean;

}