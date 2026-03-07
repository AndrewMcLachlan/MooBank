import { createFileRoute, redirect } from "@tanstack/react-router";

export const Route = createFileRoute("/shares/$id/")({
    beforeLoad: ({ params }) => {
        throw redirect({
            to: "/shares/$id/transactions",
            params: { id: params.id },
        } as any);
    },
});
