import React from "react";

import { QueryClient } from "react-query";
import { createRoot } from "react-dom/client";
import { Provider as ReduxProvider } from "react-redux";

import { library } from "@fortawesome/fontawesome-svg-core";
import { faCheckCircle, faTrashAlt, faChevronDown, faChevronUp, faTimesCircle, faArrowLeft, faChevronRight, faCircleChevronLeft, faUpload, faXmark } from "@fortawesome/free-solid-svg-icons";

import App from "./App";
import * as serviceWorker from "./serviceWorker";
import { AppProvider } from "./providers";
import { MooApp } from "@andrewmclachlan/mooapp";
import { AppStore } from "./store/configureStore";

library.add(faCheckCircle, faTrashAlt, faChevronDown, faChevronUp, faTimesCircle, faArrowLeft, faChevronRight, faCircleChevronLeft, faUpload, faXmark);

const root = createRoot(document.getElementById("root")!);

root.render(
    <AppProvider appName="MooBank" // Array.from(document.getElementsByTagName("meta")).find((value) => value.getAttribute("name") === "application-name").getAttribute("content"),
        baseUrl="/" //document.getElementsByTagName("base")[0].getAttribute("href"),
        skin="moobank" // Array.from(document.getElementsByTagName("meta")).find((value) => value.getAttribute("name") === "skin").getAttribute("content"),
    >
        <MooApp clientId="045f8afa-70f2-4700-ab75-77ac41b306f7" scopes={["api://bankplus.mclachlan.family/api.read"]}>
            <ReduxProvider store={AppStore}>
                <App />
            </ReduxProvider>
        </MooApp>
    </AppProvider>
);



// If you want your app to work offline and load faster, you can change
// unregister() to register() below. Note this comes with some pitfalls.
// Learn more about service workers: https://bit.ly/CRA-PWA
serviceWorker.unregister();
