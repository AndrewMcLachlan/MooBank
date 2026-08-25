import type { UtilityType, UsageType } from "api/types.gen";

export const UsageTypes: UsageType[] = ["Consumption", "Export"];

export const UtilityTypes: UtilityType[] = ["Electricity", "Gas", "Water", "Phone", "Internet", "Other"];

export interface CreateServiceCharge {
    chargeTypeId: number;
    chargePerDay: number;
}

export interface CreateUsage {
    usageType: UsageType;
    pricePerUnit: number;
    totalUsage: number;
}

export interface CreatePeriod {
    periodStart: string;
    periodEnd: string;
    usages: CreateUsage[];
    serviceCharges: CreateServiceCharge[];
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
