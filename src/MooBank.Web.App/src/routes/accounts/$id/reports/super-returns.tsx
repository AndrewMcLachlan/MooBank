import { createFileRoute } from "@tanstack/react-router";
import { SuperReturns } from "./-components/SuperReturns";

export const Route = createFileRoute("/accounts/$id/reports/super-returns")({
    component: SuperReturns,
});
