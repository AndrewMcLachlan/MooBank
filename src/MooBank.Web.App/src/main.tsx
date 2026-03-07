
import { createRoot } from "react-dom/client";

import { library } from "@fortawesome/fontawesome-svg-core";
import { faArrowsRotate, faCheck, faCheckCircle, faTrashAlt, faChevronDown, faChevronUp, faArrowLeft, faChevronRight, faCircleChevronLeft, faDoorClosed, faLongArrowUp, faLongArrowDown, faUpload, faXmark, faFilterCircleXmark, faInfoCircle, faPenToSquare, faPlus, faUser } from "@fortawesome/free-solid-svg-icons";

import * as serviceWorker from "./serviceWorkerRegistration";
import { StrictMode } from "react";
import { App } from "App";

library.add(faArrowsRotate, faCheck, faCheckCircle, faTrashAlt, faChevronDown, faChevronUp, faArrowLeft, faDoorClosed, faLongArrowUp, faLongArrowDown, faChevronRight, faCircleChevronLeft, faUpload, faXmark, faFilterCircleXmark, faInfoCircle, faPenToSquare, faPlus, faUser);

createRoot(document.getElementById("root")!).render(
    <StrictMode>
        <App />
    </StrictMode>
);

// If you want your app to work offline and load faster, you can change
// unregister() to register() below. Note this comes with some pitfalls.
// Learn more about service workers: https://bit.ly/CRA-PWA
serviceWorker.register();
