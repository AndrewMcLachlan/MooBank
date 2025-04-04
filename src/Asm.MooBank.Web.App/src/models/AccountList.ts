import { InstitutionAccount } from ".";

export interface AccountList {
    groups: AccountListGroup[],
    total: number,
}

export interface AccountListGroup {
    name: string,
    instruments: InstitutionAccount[],
    showTotal: boolean,
    total?: number
}
