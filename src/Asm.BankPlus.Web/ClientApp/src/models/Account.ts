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

export type accountId = string;

export interface Account {
    id: accountId;
    name: string;
    description?: string;
    currentBalance: number;
    availableBalance: number;
    balanceUpdated: Date;
    accountType: AccountType;
    controller: AccountController;
    includeInPosition: boolean;
    importerTypeId?: number;
}

export interface ImportAccount {
    importerTypeId: number;
}
