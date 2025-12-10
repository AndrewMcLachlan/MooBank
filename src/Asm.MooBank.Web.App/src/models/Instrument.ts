import { VirtualInstrument } from "./VirtualAccount";

export const Controllers = ["Manual", "Virtual", "Import"] as Controller[];
export type Controller = "Manual" | "Virtual" | "Import";

export type InstrumentId = string;

export interface Instrument {
    id: InstrumentId;
    name: string;
    controller: Controller;
    description?: string;
    currentBalance: number;
    currentBalanceLocalCurrency: number;
    currency: string;
    instrumentType?: string;
    virtualInstruments: VirtualInstrument[];
    remainingBalance?: number;
    remainingBalanceLocalCurrency?: number;
}

export interface TopLevelAccount {
    shareWithFamily: boolean;
    groupId: string;
}
