import { InstitutionAccount } from ".";

export interface AccountList {
    groups: AccountListGroup[],
    position: number,
}

export interface AccountListGroup {
    name: string,
    accounts: InstitutionAccount[],
    position?: number
}
