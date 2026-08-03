import { createFileRoute, redirect } from "@tanstack/react-router";
import { currentBudgetYear } from "./-utils/budgetYear";

export const Route = createFileRoute("/budget/")({
    beforeLoad: () => {
        throw redirect({ to: "/budget/$year", params: { year: String(currentBudgetYear()) } } as any);
    },
});
