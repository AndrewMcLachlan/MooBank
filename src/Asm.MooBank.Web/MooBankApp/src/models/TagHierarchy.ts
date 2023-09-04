import { Tag } from "./Tag";

export interface TagHierarchy {
    levels: Record<number, number>;
    tags: Tag[];
}