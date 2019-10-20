import "./index.css";

import React from "react";
import ReactDOM from "react-dom";

import { library } from "@fortawesome/fontawesome-svg-core";
import { faCheckCircle, faTrashAlt, faChevronDown } from "@fortawesome/free-solid-svg-icons";

import App from "./App";
import * as serviceWorker from "./serviceWorker";
import { SecurityService } from "services/SecurityService";

library.add(faCheckCircle, faTrashAlt, faChevronDown);

const securityService: SecurityService = new SecurityService();
if (securityService.isUserLoggedIn()) {
    ReactDOM.render(<App />, document.getElementById("root"));
} else {
    securityService.login();
}


// If you want your app to work offline and load faster, you can change
// unregister() to register() below. Note this comes with some pitfalls.
// Learn more about service workers: https://bit.ly/CRA-PWA
serviceWorker.unregister();
