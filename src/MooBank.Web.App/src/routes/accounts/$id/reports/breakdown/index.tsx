import { createFileRoute } from "@tanstack/react-router";
import { BreakdownPage } from "../-components/BreakdownPage";

export const Route = createFileRoute("/accounts/$id/reports/breakdown/")({
    component: BreakdownPage,
});
