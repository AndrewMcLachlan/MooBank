import { createFileRoute, Outlet } from "@tanstack/react-router";

export const Route = createFileRoute("/accounts/$id/reports")({
    component: () => <Outlet />,
});
