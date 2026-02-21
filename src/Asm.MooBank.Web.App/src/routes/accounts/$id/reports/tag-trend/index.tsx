import { createFileRoute } from "@tanstack/react-router";
import { TagTrend } from "../-components/TagTrend";

export const Route = createFileRoute("/accounts/$id/reports/tag-trend/")({
    component: TagTrend,
});
