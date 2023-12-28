import { emptyGuid } from "@andrewmclachlan/mooapp";

export interface RecurringTransaction {
    id: string;
    description?: string;
    amount: number;
    schedule: Schedule;
    lastRun?: string;
    nextRun: string;
    virtualAccountId: string;
}

export const emptyRecurringTransaction = (virtualAccountId: string): RecurringTransaction => ( {
    id: emptyGuid,
    description: "",
    amount: 0,
    schedule: "Weekly",
    lastRun: null,
    nextRun: "",
    virtualAccountId: virtualAccountId,
});

export const Schedules = ["Daily", "Weekly", "Monthly",  "Yearly"] as Schedule[];
export type Schedule = "Daily" | "Weekly" | "Monthly" | "Yearly";