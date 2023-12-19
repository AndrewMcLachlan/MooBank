import { emptyGuid } from "@andrewmclachlan/mooapp";

export interface Family {
    id: string;
    name: string;
}

export const emptyFamily: Family = {
    id: emptyGuid,
    name: "",
};