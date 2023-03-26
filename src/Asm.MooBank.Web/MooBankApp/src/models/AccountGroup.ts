import { emptyGuid } from "@andrewmclachlan/mooapp";

export interface AccountGroup {
    id: string;
    name: string;
    description?: string;
    showPosition: boolean;
}

export const emptyAccountGroup : AccountGroup = {
    id: emptyGuid,
    name: "",
    showPosition: true,
}