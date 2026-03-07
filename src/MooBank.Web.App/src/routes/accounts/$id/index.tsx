import { createFileRoute, redirect } from "@tanstack/react-router";

export const Route = createFileRoute("/accounts/$id/")({
    beforeLoad: ({ params, location }) => {
        throw redirect({
            to: "/accounts/$id/transactions",
            params: { id: params.id },
            search: location.search,
        } as any);
    },
});
