export enum AccountType {
    NotSet = 0,
    Transaction = 1,
    Savings = 2,
}

export enum AccountController {
    Manual = 0,
    VirtualAccount = 1,
    Import = 2,
}

export interface Account {
    id: string;
    name: string;
    description?: string;
    currentBalance: number;
    availableBalance: number;
    balanceUpdated: Date;
    accountType: AccountType;
    controller: AccountController;
}