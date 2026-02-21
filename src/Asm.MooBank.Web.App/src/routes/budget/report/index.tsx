import { createFileRoute } from "@tanstack/react-router";
import { BudgetReport } from "../-components/BudgetReport";

export const Route = createFileRoute("/budget/report/")({
    component: BudgetReport,
});
