import { SortDirection } from "@andrewmclachlan/mooapp";
import { Tag } from ".";

export interface TransactionTagRule {
    id: number;
    contains: string;
    description?: string;
    tags: Tag[];
}

export const sortRules = (sortDirection: SortDirection) => (a: TransactionTagRule, b: TransactionTagRule) => {

    const retVal = sortDirection === "Ascending" ? 1 : -1;
    const aName = a.contains.toUpperCase();
    const bName = b.contains.toUpperCase();

    if (aName === bName ) return 0;
    if (aName > bName) return retVal;
    if (aName < bName) return -retVal;

}