import { SortDirection } from "@andrewmclachlan/mooapp";

export interface Tag {
    id: number;
    name: string;
    tags: Tag[];
    settings?: TagSettings;
}

export interface TagSettings {
    applySmoothing: boolean;
    excludeFromReporting: boolean;
}

export const compareTags = (left: Tag, right: Tag):boolean => {
    if (!left && !right) return true;
    if (!left || !right) return false;    

    return left.id === right.id && left.name === right.name && left.tags.length === right.tags.length;
}

export const compareTagArray = (left: Tag[], right: Tag[]) => {
        if (!left && !right) return true;
        if (!left || !right) return false;
        if (left.length !== right.length) return false;
        for (let i = 0; i<left.length;i++) {
            if (!compareTags(left[i], right[i])) {
                return false;
            }
        }

        return true;
}

export const sortTags = (sortDirection: SortDirection) => (a: Tag, b: Tag) => {

    const retVal = sortDirection === "Ascending" ? 1 : -1;
    const aName = a.name.toUpperCase();
    const bName = b.name.toUpperCase();

    if (aName === bName ) return 0;
    if (aName > bName) return retVal;
    if (aName < bName) return -retVal;

}