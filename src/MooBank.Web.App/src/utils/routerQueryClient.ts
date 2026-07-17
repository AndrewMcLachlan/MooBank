import type { QueryClient } from "@tanstack/react-query";

// PROTOTYPE bridge for the /accounts/$id route loader.
//
// A TanStack Router `loader` runs outside React, so it cannot call
// `useQueryClient()`. MooApp creates the QueryClient internally and only exposes
// it through React context — unreachable from a loader — so the root route
// captures it here for the loader to warm the account cache.
//
// PRODUCTION shape: MooApp should inject its QueryClient into the router context
// (`<RouterProvider router={router} context={{ queryClient }} />`) and the root
// route use `createRootRouteWithContext<{ queryClient: QueryClient }>()`. The
// loader would then read `context.queryClient` and this module would go away.
let routerQueryClient: QueryClient | undefined;

export const setRouterQueryClient = (client: QueryClient): void => {
    routerQueryClient = client;
};

export const getRouterQueryClient = (): QueryClient | undefined => routerQueryClient;
