import { createFileRoute } from "@tanstack/react-router";
import { PrincipalVsInterest } from "./-components/PrincipalVsInterest";

export const Route = createFileRoute("/accounts/$id/reports/principal-vs-interest")({
    component: PrincipalVsInterest,
});
