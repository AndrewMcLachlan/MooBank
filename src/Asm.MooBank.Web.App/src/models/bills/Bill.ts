import { Period } from "./Period";

export interface Bill {
    id: number;
    accountId: string;
    accountName: string;
    invoiceNumber: string;
    issueDate: string;
    currentReading?: number;
    previousReading?: number;
    total: number;
    costsIncludeGST?: boolean;
    cost: number;
    periods?: Period[];
}
