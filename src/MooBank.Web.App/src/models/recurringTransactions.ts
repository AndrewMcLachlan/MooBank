import type { RecurringTransactionDetails, ScheduleFrequency } from "api/types.gen";

export const Schedules: ScheduleFrequency[] = ["Daily", "Weekly", "Monthly", "Yearly"];

// The ids live in the route, so a new recurring transaction is just its mutable fields.
export const emptyRecurringTransaction = (): RecurringTransactionDetails => ({
    description: "",
    amount: 0,
    schedule: "Weekly",
    nextRun: "",
});
