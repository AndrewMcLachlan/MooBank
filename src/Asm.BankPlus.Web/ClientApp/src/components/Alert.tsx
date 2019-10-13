import React from "react";
import { Alert as BSAlert} from "react-bootstrap";
import { useSelector } from "react-redux";

import { State } from "../store/state";

export const Alert: React.FC = () => {
    
    const message = useSelector((state: State) => state.app.message);

    if (!message || message === "") {
        return null;
    }

    return (
         <BSAlert variant="danger">{message}</BSAlert>
    );
}