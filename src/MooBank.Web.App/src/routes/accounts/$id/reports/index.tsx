import { createFileRoute, redirect } from "@tanstack/react-router";

export const Route = createFileRoute("/accounts/$id/reports/")({
    beforeLoad: ({ params }) => {
        throw redirect({
            to: "/accounts/$id/reports/in-out",
            params: { id: params.id },
        } as any);
    },
});
