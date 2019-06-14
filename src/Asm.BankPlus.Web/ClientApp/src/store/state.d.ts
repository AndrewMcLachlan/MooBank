import Msal, { UserAgentApplication } from "msal";

export interface State {
    app: App,
    accounts: Accounts,
    security: Security,
}

export interface App {
    appName: string;
    skin: string;
    baseUrl: string;
}

export interface Accounts {
    accounts: Array<RealAccount>;
    virtualAccounts: Array<VirtualAccount>;
    areLoading: boolean;
}

export interface Security {
    loggedIn: boolean;
    msal: UserAgentApplication;
}

export interface RealAccount {
    accountId: string;
    name: string;
    accountBalance: number;
    availableBalance: number;
    updateVirtualAccount: boolean;
    lastUpdated: Date;
}

export interface VirtualAccount {
    virtualAccountId: string;
    name: string;
    description: string;
    balance: number;
    defaultAccount: boolean;
    closed: boolean;

}