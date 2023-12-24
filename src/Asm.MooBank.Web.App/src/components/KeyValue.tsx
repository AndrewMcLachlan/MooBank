import { HTMLAttributes, PropsWithChildren } from "react";

export const KeyValue: React.FC<PropsWithChildren<HTMLAttributes<HTMLElement>>> = ({ children }) => (
    <div className="key-value">
        {children}
    </div>
);