import "~/treeflex/dist/css/treeflex.css";

import { Provider as ReduxProvider } from "react-redux";

import { MooApp, createMooAppBrowserRouter } from "@andrewmclachlan/moo-app";
import { AppStore } from "./store/configureStore";
import { routes } from "Routes";
import { client } from "./api/client.gen";

export const App = () => {

    const router = createMooAppBrowserRouter(routes);

    return (
        <ReduxProvider store={AppStore}>
            <MooApp client={client.instance} clientId="045f8afa-70f2-4700-ab75-77ac41b306f7" scopes={["api://moobank.mclachlan.family/api.read"]} name="MooBank" version={import.meta.env.VITE_REACT_APP_VERSION} copyrightYear={2013} router={router} />
        </ReduxProvider>
    );
}