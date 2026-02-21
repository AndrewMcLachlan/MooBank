import { createFileRoute, Outlet } from "@tanstack/react-router";

export const Route = createFileRoute("/shares/$id/reports")({
    component: () => <Outlet />,
});
