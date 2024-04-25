import { emptyGuid } from "@andrewmclachlan/mooapp";

export interface Group {
    id: string;
    name: string;
    description?: string;
    showPosition: boolean;
}

export const emptyGroup : Group = {
    id: emptyGuid,
    name: "",
    showPosition: true,
}
