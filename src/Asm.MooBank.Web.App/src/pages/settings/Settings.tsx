import React from "react";
import { Navigate, Outlet, PathPattern, useMatch } from "react-router-dom";

export const Settings: React.FC = () => {

    const pattern: PathPattern = {
        path: "/settings",
        end: true,
    };
    const match = useMatch(pattern);

    if (match)
    {
        return <Navigate to={`/settings/families`} replace />
    }

    return <Outlet />;
}