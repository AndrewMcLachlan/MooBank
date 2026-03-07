import { emptyGuid } from "@andrewmclachlan/moo-ds";
import type { Group } from "api/types.gen";

export const emptyGroup: Group = {
    id: emptyGuid,
    name: "",
    showTotal: true,
};
