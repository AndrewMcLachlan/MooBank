import { emptyGuid } from "@andrewmclachlan/mooapp";

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
