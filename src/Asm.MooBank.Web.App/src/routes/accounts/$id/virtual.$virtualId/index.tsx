import { createFileRoute, redirect } from "@tanstack/react-router";

export const Route = createFileRoute("/accounts/$id/virtual/$virtualId/")({
    beforeLoad: ({ params }) => {
        throw redirect({
            to: "/accounts/$id/virtual/$virtualId/transactions",
            params: { id: params.id, virtualId: params.virtualId },
        } as any);
    },
});
