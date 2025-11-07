import { LogicalAccount } from ".";

export interface AccountList {
    groups: AccountListGroup[],
    total: number,
}

export interface AccountListGroup {
    name: string,
    instruments: LogicalAccount[],
    showTotal: boolean,
    total?: number
}
