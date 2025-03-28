import "~/treeflex/dist/css/treeflex.css";

import { createRoot } from "react-dom/client";
import { Provider as ReduxProvider } from "react-redux";

import { library } from "@fortawesome/fontawesome-svg-core";
import { faArrowsRotate, faCheck, faCheckCircle, faTrashAlt, faChevronDown, faChevronUp, faTimesCircle, faArrowLeft, faChevronRight, faCircleChevronLeft, faLongArrowUp, faLongArrowDown, faUpload, faXmark, faFilterCircleXmark, faInfoCircle, faPenToSquare, faPlus, faUser } from "@fortawesome/free-solid-svg-icons";

import * as serviceWorker from "./serviceWorkerRegistration";
import { MooApp, createMooAppBrowserRouter } from "@andrewmclachlan/mooapp";
import { AppStore } from "./store/configureStore";
import { routes } from "Routes";
import { RouterProvider } from "react-router";

library.add(faArrowsRotate, faCheck, faCheckCircle, faTrashAlt, faChevronDown, faChevronUp, faArrowLeft, faLongArrowUp, faLongArrowDown, faChevronRight, faCircleChevronLeft, faUpload, faXmark, faFilterCircleXmark, faInfoCircle, faPenToSquare, faPlus, faUser);

const root = createRoot(document.getElementById("root")!);

const versionMeta = Array.from(document.getElementsByTagName("meta")).find((value) => value.getAttribute("name") === "application-version");
versionMeta.content = import.meta.env.VITE_REACT_APP_VERSION;

const router = createMooAppBrowserRouter(routes);

root.render(
    <MooApp clientId="045f8afa-70f2-4700-ab75-77ac41b306f7" scopes={["api://moobank.mclachlan.family/api.read"]} name="MooBank" version={import.meta.env.VITE_REACT_APP_VERSION} copyrightYear={2013}>
        <ReduxProvider store={AppStore}>
            <RouterProvider router={router} />
        </ReduxProvider>
    </MooApp>
);



// If you want your app to work offline and load faster, you can change
// unregister() to register() below. Note this comes with some pitfalls.
// Learn more about service workers: https://bit.ly/CRA-PWA
serviceWorker.register();
