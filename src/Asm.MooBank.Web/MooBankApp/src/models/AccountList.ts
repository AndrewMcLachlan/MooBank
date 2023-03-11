import { Account } from ".";

export interface AccountList {
    positionedAccounts: Account[],
    otherAccounts: Account[],
    position: number,
}
