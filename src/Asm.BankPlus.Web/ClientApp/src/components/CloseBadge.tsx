import "./CloseBadge.scss";

import React, { PropsWithChildren } from "react";
import { Badge, BadgeProps } from "react-bootstrap";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";

export const CloseBadge: React.FC<PropsWithChildren<CloseBadgeProps>> = (props) => {

    const click = (e:React.MouseEvent<any, any>) => {
        e.defaultPrevented = true;
        e.stopPropagation();

        props.onClose && props.onClose();
    }

    let { onClick, className, ...other } = props;

    className = (className + " close-badge").trim();

    return (<Badge {...other} className={className} >{props.children}<span onClick={click}><FontAwesomeIcon icon="times-circle" /></span></Badge>);
}

CloseBadge.displayName = "CloseBadge";

export interface CloseBadgeProps extends BadgeProps {
    onClose?: () => void;
}
