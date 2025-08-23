import { emptyGuid } from "@andrewmclachlan/moo-ds";

export interface Group {
    id: string;
    name: string;
    description?: string;
    showTotal: boolean;
}

export const emptyGroup : Group = {
    id: emptyGuid,
    name: "",
    showTotal: true,
}
