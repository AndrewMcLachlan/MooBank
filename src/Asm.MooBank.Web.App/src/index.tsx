import "~/treeflex/dist/css/treeflex.css";

import { createRoot } from "react-dom/client";
import { Provider as ReduxProvider } from "react-redux";

import { library } from "@fortawesome/fontawesome-svg-core";
import { faArrowsRotate, faCheck, faCheckCircle, faTrashAlt, faChevronDown, faChevronUp, faTimesCircle, faArrowLeft, faChevronRight, faCircleChevronLeft, faLongArrowUp, faLongArrowDown, faUpload, faXmark, faFilterCircleXmark, faInfoCircle, faPenToSquare, faPlus } from "@fortawesome/free-solid-svg-icons";

import App from "./App";
import * as serviceWorker from "./serviceWorkerRegistration";
import { MooApp } from "@andrewmclachlan/mooapp";
import { AppStore } from "./store/configureStore";

library.add(faArrowsRotate, faCheck, faCheckCircle, faTrashAlt, faChevronDown, faChevronUp, faTimesCircle, faArrowLeft, faLongArrowUp, faLongArrowDown, faChevronRight, faCircleChevronLeft, faUpload, faXmark, faFilterCircleXmark, faInfoCircle, faPenToSquare, faPlus);

const root = createRoot(document.getElementById("root")!);

const versionMeta = Array.from(document.getElementsByTagName("meta")).find((value) => value.getAttribute("name") === "application-version");
versionMeta.content = import.meta.env.VITE_REACT_APP_VERSION;

root.render(
    <MooApp clientId="045f8afa-70f2-4700-ab75-77ac41b306f7" scopes={["api://moobank.mclachlan.family/api.read"]} name="MooBank" version={import.meta.env.VITE_REACT_APP_VERSION}>
        <ReduxProvider store={AppStore}>
            <App />
        </ReduxProvider>
    </MooApp>
);



// If you want your app to work offline and load faster, you can change
// unregister() to register() below. Note this comes with some pitfalls.
// Learn more about service workers: https://bit.ly/CRA-PWA
serviceWorker.register();
