import { createFileRoute } from "@tanstack/react-router";
import { Dashboard } from "./-dashboard/Dashboard";

export const Route = createFileRoute("/")({
    component: Dashboard,
});
