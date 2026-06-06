import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/accounts/$id/reports/breakdown/$tagId")({
    component: () => null,
});
