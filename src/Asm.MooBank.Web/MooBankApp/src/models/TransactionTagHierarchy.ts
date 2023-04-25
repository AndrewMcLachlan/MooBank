import { TransactionTag } from "./TransactionTag";

export interface TransactionTagHierarchy {
    levels: Record<number, number>;
    tags: TransactionTag[];
}