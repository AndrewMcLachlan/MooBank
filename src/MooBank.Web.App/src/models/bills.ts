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

/**
 * The values a bill form edits. Creating and updating take the same shape.
 *
 * There is no cost or total: the database derives the cost from the periods and the total from the
 * readings, so both are ignored on the way in.
 */
export interface CreateBill {
    invoiceNumber?: string;
    issueDate: string;
    currentReading?: number;
    previousReading?: number;
    costsIncludeGST?: boolean;
    periods: CreatePeriod[];
    discounts: CreateDiscount[];
}
