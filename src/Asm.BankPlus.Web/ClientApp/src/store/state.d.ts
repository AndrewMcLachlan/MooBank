import * as Models from "../models";

export interface State {
    app: App,
    accounts?: Accounts,
    transactions?: Transactions,
    security?: Security,
}

export interface App {
    baseUrl: string;
    message?: string;
}

export interface Accounts {
    accounts: Models.Account[];
    virtualAccounts: VirtualAccount[];
    areLoading: boolean;
    selectedAccount?: Models.Account;
    position: number;
}

export interface Transactions {
    areLoading: boolean;
    currentPage: number;
    pageSize: number;
    total: number;
    filterTagged: boolean;
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
