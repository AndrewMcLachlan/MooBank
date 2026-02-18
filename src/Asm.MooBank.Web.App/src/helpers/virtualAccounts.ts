import type { LogicalAccount, VirtualInstrument } from "api/types.gen";

export interface UpdateVirtualInstrument {
    name?: string;
    description?: string;
}

export const isVirtualInstrument = (account: LogicalAccount | VirtualInstrument): boolean =>
    !!(account as VirtualInstrument).parentId;
