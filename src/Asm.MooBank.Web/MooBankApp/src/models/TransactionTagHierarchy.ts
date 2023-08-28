import { Tag } from "./TransactionTag";

export interface TagHierarchy {
    levels: Record<number, number>;
    tags: Tag[];
}