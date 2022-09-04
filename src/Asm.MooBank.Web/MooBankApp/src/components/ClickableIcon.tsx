import React from "react";
import { FontAwesomeIcon, FontAwesomeIconProps } from "@fortawesome/react-fontawesome";

export const ClickableIcon: React.FC<ClickableIconProps> = (props) => {

    const className = (props.className + " clickable").trim();

    return <FontAwesomeIcon {...props} className={className} />;
}

ClickableIcon.displayName = "ClickableIcon";

export interface ClickableIconProps extends FontAwesomeIconProps {
}
