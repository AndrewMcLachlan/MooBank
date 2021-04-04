import "./index.css";

import React from "react";
import ReactDOM from "react-dom";

import { library } from "@fortawesome/fontawesome-svg-core";
import { faCheckCircle, faTrashAlt, faChevronDown, faTimesCircle, faArrowLeft } from "@fortawesome/free-solid-svg-icons";

import App from "./App";
import * as serviceWorker from "./serviceWorker";
import { MsalProvider, msalConfig } from "./components";

library.add(faCheckCircle, faTrashAlt, faChevronDown, faTimesCircle,faArrowLeft);

ReactDOM.render(
    (
        <MsalProvider config={msalConfig}>
            <App />
        </MsalProvider>
    ), document.getElementById("root"));



// If you want your app to work offline and load faster, you can change
// unregister() to register() below. Note this comes with some pitfalls.
// Learn more about service workers: https://bit.ly/CRA-PWA
serviceWorker.unregister();
