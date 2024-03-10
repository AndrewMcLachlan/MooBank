export interface AccountHolder {
    id: string,
    emailAddress : string,
    firstName?: string,
    lastName?: string,
    currency: string
    familyId: string,
    primaryAccountId: string,
    cards: Card[]
}

export interface Card {
    last4Digits: number,
    name: string
}
