import { LogicalAccount } from ".";

export interface AccountList {
    groups: AccountListGroup[],
    total: number,
}

export interface AccountListGroup {
    id?: string,
    name: string,
    instruments: LogicalAccount[],
    showTotal: boolean,
    total?: number
}
