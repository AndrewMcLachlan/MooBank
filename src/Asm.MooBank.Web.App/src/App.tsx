import "~/treeflex/dist/css/treeflex.css";
import "utils/chartSetup";

import { Provider as ReduxProvider } from "react-redux";

import { MooApp } from "@andrewmclachlan/moo-app";
import { createRouter } from "@tanstack/react-router";
import { Spinner } from "@andrewmclachlan/moo-ds";
import { AppStore } from "./store/configureStore";
import { routeTree } from "./routeTree.gen.ts";
import { client } from "./api/client.gen";

export const App = () => {

    // @ts-expect-error strictNullChecks is false — TanStack Router requires it for full type safety
    const router = createRouter({
        routeTree,
        defaultPreload: "intent",
        defaultPreloadStaleTime: 0,
        scrollRestoration: true,
        defaultPendingComponent: Spinner,
    })

    return (
        <ReduxProvider store={AppStore}>
            <MooApp client={client.instance} clientId="045f8afa-70f2-4700-ab75-77ac41b306f7" scopes={["api://moobank.mclachlan.family/api.read"]} name="MooBank" version={import.meta.env.VITE_REACT_APP_VERSION} copyrightYear={2013} router={router} />
        </ReduxProvider>
    );
}