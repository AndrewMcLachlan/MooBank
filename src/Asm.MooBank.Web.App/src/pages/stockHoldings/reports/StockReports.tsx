import React from "react";
import { Outlet, useLocation, Navigate } from "react-router";

export const StockReports = () => {

    const location = useLocation();

    if (location.pathname.endsWith("reports"))
    {
        return <Navigate to="value" replace />
    }

    return (
        <Outlet />
    );
};
