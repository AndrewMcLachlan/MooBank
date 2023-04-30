import { sortDirection } from "store/state";
import { TransactionTag } from ".";

export interface TransactionTagRule {
    id: number;
    contains: string;
    description?: string;
    tags: TransactionTag[];
}

export const sortRules = (sortDirection: sortDirection) => (a: TransactionTagRule, b: TransactionTagRule) => {

    const retVal = sortDirection === "Ascending" ? 1 : -1;
    const aName = a.contains.toUpperCase();
    const bName = b.contains.toUpperCase();

    if (aName === bName ) return 0;
    if (aName > bName) return retVal;
    if (aName < bName) return -retVal;

}