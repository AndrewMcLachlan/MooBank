import { UtilityType } from "api/types.gen";

export const UtilityTypes: UtilityType[] = ["Electricity", "Gas", "Water", "Phone", "Internet", "Other"];

export interface CreatePeriod {
    periodStart: string;
    periodEnd: string;
    pricePerUnit: number;
    totalUsage: number;
    chargePerDay: number;
}

export interface CreateDiscount {
    discountPercent?: number;
    discountAmount?: number;
    reason?: string;
}

export interface CreateBill {
    invoiceNumber?: string;
    issueDate: string;
    currentReading?: number;
    previousReading?: number;
    total: number;
    costsIncludeGST?: boolean;
    cost: number;
    periods: CreatePeriod[];
    discounts: CreateDiscount[];
}
