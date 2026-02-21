import { createFileRoute, redirect } from "@tanstack/react-router";

export const Route = createFileRoute("/assets/$id/")({
    beforeLoad: ({ params }) => {
        throw redirect({
            to: "/assets/$id/manage",
            params: { id: params.id },
        } as any);
    },
});
