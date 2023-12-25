import { HTMLAttributes, PropsWithChildren } from "react";

export const KeyValue: React.FC<PropsWithChildren<HTMLAttributes<HTMLElement>>> = ({ children, ...props }) => (
    <div className="key-value" {...props}>
        {children}
    </div>
);