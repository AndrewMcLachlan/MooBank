import { emptyGuid } from "@andrewmclachlan/moo-ds";
import { Family } from "api/types.gen";

export const emptyFamily: Family = {
    id: emptyGuid,
    name: "",
    members: [],
};
