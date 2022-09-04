import { TransactionTag } from ".";

export interface TransactionTagRule {
    id: number;
    contains: string;
    tags: TransactionTag[];
}