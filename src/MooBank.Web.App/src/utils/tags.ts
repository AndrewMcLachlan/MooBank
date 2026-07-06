import type { SortDirection } from "@andrewmclachlan/moo-ds";
import type { Tag } from "api/types.gen";

export const sortTags = (sortDirection: SortDirection) => (a: Tag, b: Tag) => {

    const retVal = sortDirection === "Ascending" ? 1 : -1;
    const aName = a.name.toUpperCase();
    const bName = b.name.toUpperCase();

    if (aName === bName) return 0;
    if (aName > bName) return retVal;
    if (aName < bName) return -retVal;
    return 0;
};
