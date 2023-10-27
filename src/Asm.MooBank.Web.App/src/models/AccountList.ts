import { Account } from ".";

export interface AccountList {
    accountGroups: AccountListGroup[],
    position: number,
}

export interface AccountListGroup {
    name: string,
    accounts: Account[],
    position?: number
}