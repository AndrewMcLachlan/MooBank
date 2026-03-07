import { createFileRoute, redirect } from "@tanstack/react-router";

export const Route = createFileRoute("/shares/$id/reports/")({
    beforeLoad: ({ params }) => {
        throw redirect({
            to: "/shares/$id/reports/value",
            params: { id: params.id },
        } as any);
    },
});
