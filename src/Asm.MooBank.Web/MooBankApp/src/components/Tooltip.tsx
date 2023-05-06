import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { PropsWithChildren, useState } from "react";
import { Tooltip as BSTooltip, OverlayTrigger } from "react-bootstrap";

export const Tooltip: React.FC<PropsWithChildren<{id: string}>> = ({ id, children }) => {

    return (
        <OverlayTrigger key={id} placement="top" overlay={
            <BSTooltip id={`tooltip-${id}`}>
                {children}
            </BSTooltip>
        }>
            <span className="tooltip-icon"><FontAwesomeIcon icon="info-circle" size="1x" /></span>
        </OverlayTrigger>
    );
}