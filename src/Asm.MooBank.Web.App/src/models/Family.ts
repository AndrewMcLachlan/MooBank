import { emptyGuid } from "@andrewmclachlan/moo-ds";

export interface FamilyMember {
    id: string;
    emailAddress: string;
    firstName?: string;
    lastName?: string;
}

export interface Family {
    id: string;
    name: string;
    members?: FamilyMember[];
}

export const emptyFamily: Family = {
    id: emptyGuid,
    name: "",
    members: [],
};