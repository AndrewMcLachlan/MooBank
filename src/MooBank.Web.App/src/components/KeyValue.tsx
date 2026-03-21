import classNames from "classnames";
import type { HTMLAttributes, PropsWithChildren } from "react";

export const KeyValue: React.FC<PropsWithChildren<HTMLAttributes<HTMLElement>>> = ({ children, className, ...props }) => (
    <div className={classNames("key-value", className)} {...props}>
        {children}
    </div>
);