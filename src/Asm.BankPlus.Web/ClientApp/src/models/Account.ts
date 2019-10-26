export enum AccountType {
    None = 0,
    Transaction = 1,
    Savings = 2,
    Credit = 3,
    Mortgage = 4,
    Superannuation = 5,
}

export enum AccountController {
    Manual = 0,
    Virtual = 1,
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

export interface ImportAccount {
    importerTypeId: number;
}