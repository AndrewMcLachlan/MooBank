import { Period } from "./Period";

export interface BillBase {
    invoiceNumber: string;
    issueDate: string;
    currentReading?: number;
    previousReading?: number;
    total: number;
    costsIncludeGST?: boolean;
    cost: number;
    periods?: Period[];
}

export interface Bill extends BillBase {
    id: number;
    accountId: string;
    accountName: string;
}

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