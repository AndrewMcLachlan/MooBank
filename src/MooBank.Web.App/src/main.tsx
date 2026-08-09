
import { createRoot } from "react-dom/client";

import { library } from "@fortawesome/fontawesome-svg-core";
import { faArrowsRotate, faCheck, faCheckCircle, faTrashAlt, faChevronDown, faChevronUp, faArrowLeft, faChevronRight, faGripVertical, faCircleChevronLeft, faDoorClosed, faLongArrowUp, faLongArrowDown, faUpload, faXmark, faFilterCircleXmark, faInfoCircle, faPenToSquare, faPlus, faTriangleExclamation, faUser } from "@fortawesome/free-solid-svg-icons";

import { StrictMode } from "react";
import { App } from "App";

library.add(faArrowsRotate, faCheck, faCheckCircle, faTrashAlt, faChevronDown, faChevronUp, faArrowLeft, faGripVertical, faDoorClosed, faLongArrowUp, faLongArrowDown, faChevronRight, faCircleChevronLeft, faUpload, faXmark, faFilterCircleXmark, faInfoCircle, faPenToSquare, faPlus, faTriangleExclamation, faUser);

createRoot(document.getElementById("root")!).render(
    <StrictMode>
        <App />
    </StrictMode>
);
