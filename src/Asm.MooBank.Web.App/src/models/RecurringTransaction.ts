export interface RecurringTransaction {
    id: string;
    description?: string;
    amount: number;
    schedule: "Daily" | "Weekly" | "Monthly" | "Yearly";
    lastRun: string;
}