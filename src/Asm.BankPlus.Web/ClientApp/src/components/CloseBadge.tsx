import "./CloseBadge.scss";

import React, { PropsWithChildren } from "react";
import { Badge, BadgeProps } from "react-bootstrap";

export const CloseBadge: React.FC<PropsWithChildren<CloseBadgeProps>> = (props) => {

    const click = (e:React.MouseEvent<any, any>) => {
        e.defaultPrevented = true;
        e.stopPropagation();

        props.onClose && props.onClose();
    }

    let { onClick, className, ...other } = props;

    className = (className + " close-badge").trim();

    return (<Badge {...other} className={className} >{props.children}<span><i onClick={click} className="fa fa-close" /></span></Badge>);
}

CloseBadge.displayName = "CloseBadge";

export interface CloseBadgeProps extends BadgeProps {
    onClose?: () => void;
}
