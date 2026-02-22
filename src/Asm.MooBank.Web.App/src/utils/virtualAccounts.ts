import type { LogicalAccount, VirtualInstrument } from "api/types.gen";

export const isVirtualInstrument = (account: LogicalAccount | VirtualInstrument): boolean =>
    !!(account as VirtualInstrument).parentId;
