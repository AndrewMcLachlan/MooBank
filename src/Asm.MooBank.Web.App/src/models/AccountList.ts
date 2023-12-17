import { InstitutionAccount } from ".";

export interface AccountList {
    accountGroups: AccountListGroup[],
    position: number,
}

export interface AccountListGroup {
    name: string,
    accounts: InstitutionAccount[],
    position?: number
}