import { SortDirection } from "@andrewmclachlan/moo-ds";
import { Rule } from "api/types.gen";

export const sortRules = (sortDirection: SortDirection) => (a: Rule, b: Rule) => {

    const retVal = sortDirection === "Ascending" ? 1 : -1;
    const aName = a.contains.toUpperCase();
    const bName = b.contains.toUpperCase();

    if (aName === bName) return 0;
    if (aName > bName) return retVal;
    if (aName < bName) return -retVal;
    return 0;
};

export const emptyRule: Rule = {
    id: 0,
    contains: "",
    description: "",
    tags: []
};
