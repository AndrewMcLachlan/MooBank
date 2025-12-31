import { Controller, Instrument } from "models/Instrument";

export interface BillAccount extends Instrument {
    utilityType: UtilityType;
    firstBill: string;
    latestBill: string;
}

export interface CreateBillAccount {
    name: string;
    description?: string;
    utilityType: UtilityType;
    accountNumber: string;
    institutionId?: number;
    currency: string;
    groupId?: string;
    shareWithFamily: boolean;
    controller: Controller;
}

export const UtilityTypes = ["Electricity", "Gas", "Water", "Phone", "Internet", "Other"] as UtilityType[];
export type UtilityType = "Electricity" | "Gas" | "Water" | "Phone" | "Internet" | "Other";

export interface AccountTypeSummary {
    utilityType: UtilityType;
    from: string;
    accounts: string[];
}