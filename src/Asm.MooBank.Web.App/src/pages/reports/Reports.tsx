import React from "react";
import { Outlet, useLocation, Navigate } from "react-router";

export const Reports = () => {

    const location = useLocation();

    if (location.pathname.endsWith("reports"))
    {
        return <Navigate to="in-out" replace />
    }

    return (
        <Outlet />
    );
};
