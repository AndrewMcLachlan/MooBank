import { emptyGuid } from "@andrewmclachlan/moo-ds";
import { Group } from "api/types.gen";

export const emptyGroup: Group = {
    id: emptyGuid,
    name: "",
    showTotal: true,
};
