import * as Models from "../models";

export interface State {
    app: App,
    accounts?: Accounts,
    transactionCategories?: TransactionCategories,
    security?: Security,
}

export interface App {
    appName: string;
    skin: string;
    baseUrl: string;
}

export interface Accounts {
    accounts: Models.Account[];
    virtualAccounts: VirtualAccount[];
    areLoading: boolean;
}

export interface TransactionCategories {
    categories: Models.TransactionCategory[],
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