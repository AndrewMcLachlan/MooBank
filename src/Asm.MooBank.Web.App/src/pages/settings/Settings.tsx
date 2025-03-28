import React from "react";
import { Navigate, Outlet, PathPattern, useMatch } from "react-router";

export const Settings: React.FC = () => {

    const pattern: PathPattern = {
        path: "/settings",
        end: true,
    };
    const match = useMatch(pattern);

    if (match) {
        return <Navigate to={`/settings/institutions`} replace />
    }

    return <Outlet />;
}
