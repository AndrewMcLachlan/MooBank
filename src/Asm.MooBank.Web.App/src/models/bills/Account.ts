import { Instrument } from "models/Instrument";

export interface BillAccount extends Instrument {
    utilityType: UtilityType;
    firstBill: string;
    latestBill: string;
}

export const UtilityTypes = ["Electricity", "Gas", "Water", "Phone", "Internet", "Other"] as UtilityType[];
export type UtilityType = "Electricity" | "Gas" | "Water" | "Phone" | "Internet" | "Other";

export interface AccountTypeSummary {
    utilityType: UtilityType;
    from: string;
    accounts: string[];
}