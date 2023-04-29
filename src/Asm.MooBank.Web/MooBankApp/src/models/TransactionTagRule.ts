import { TransactionTag } from ".";

export interface TransactionTagRule {
    id: number;
    contains: string;
    description?: string;
    tags: TransactionTag[];
}