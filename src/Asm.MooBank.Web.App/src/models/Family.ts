import { emptyGuid } from "@andrewmclachlan/moo-ds";

export interface Family {
    id: string;
    name: string;
}

export const emptyFamily: Family = {
    id: emptyGuid,
    name: "",
};