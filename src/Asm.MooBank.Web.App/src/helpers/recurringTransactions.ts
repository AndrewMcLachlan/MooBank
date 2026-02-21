import { emptyGuid } from "@andrewmclachlan/moo-ds";
import type { RecurringTransaction, ScheduleFrequency } from "api/types.gen";

export const Schedules: ScheduleFrequency[] = ["Daily", "Weekly", "Monthly", "Yearly"];

export const emptyRecurringTransaction = (virtualAccountId: string): RecurringTransaction => ({
    id: emptyGuid,
    description: "",
    amount: 0,
    schedule: "Weekly",
    lastRun: null,
    nextRun: "",
    virtualAccountId: virtualAccountId,
});
