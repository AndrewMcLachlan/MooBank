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

export interface CreateBill extends BillBase {
}