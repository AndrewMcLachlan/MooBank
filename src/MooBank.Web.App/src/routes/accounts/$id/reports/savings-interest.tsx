import { createFileRoute } from "@tanstack/react-router";
import { SavingsInterest } from "./-components/SavingsInterest";

export const Route = createFileRoute("/accounts/$id/reports/savings-interest")({
    component: SavingsInterest,
});
