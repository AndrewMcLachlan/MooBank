export interface TransactionTag {
    id: number;
    name: string;
    tags: TransactionTag[];
}

export const compareTransactionTags = (left: TransactionTag, right: TransactionTag):boolean => {
    if (!left && !right) return true;
    if (!left || !right) return false;    

    return left.id === right.id && left.name === right.name && left.tags.length === right.tags.length;
}

export const compareTransactionTagArray = (left: TransactionTag[], right: TransactionTag[]) => {
        if (!left && !right) return true;
        if (!left || !right) return false;
        if (left.length !== right.length) return false;
        for (let i = 0; i<left.length;i++) {
            if (!compareTransactionTags(left[i], right[i])) {
                return false;
            }
        }

        return true;
}

