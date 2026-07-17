import { createFileRoute, redirect } from "@tanstack/react-router";

// Fallback for direct hits on the bare /accounts/$id URL (bookmarks, refresh): redirect to the
// default tab, forwarding any search params. In-app navigation must target /accounts/$id/transactions
// (or another concrete tab) DIRECTLY, never the bare route — a client-side navigation through this
// beforeLoad redirect deadlocks the router if the /accounts/$id layout route ever gains a loader.
export const Route = createFileRoute("/accounts/$id/")({
    beforeLoad: ({ params, location }) => {
        throw redirect({
            to: "/accounts/$id/transactions",
            params: { id: params.id },
            search: location.search,
        } as any);
    },
});
