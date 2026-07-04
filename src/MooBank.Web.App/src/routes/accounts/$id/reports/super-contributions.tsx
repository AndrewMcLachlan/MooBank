import { createFileRoute } from "@tanstack/react-router";
import { SuperContributions } from "./-components/SuperContributions";

export const Route = createFileRoute("/accounts/$id/reports/super-contributions")({
    component: SuperContributions,
});
