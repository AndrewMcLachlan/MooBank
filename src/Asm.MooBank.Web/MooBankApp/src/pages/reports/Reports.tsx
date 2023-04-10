import React from "react";
import { matchRoutes, Outlet, useNavigate, useLocation } from "react-router";

export const Reports = () => {

    const location = useLocation();
    const navigate = useNavigate();

    if (location.pathname.endsWith("reports"))
    {
        navigate("in-out", { replace: true });
    }

    return (
        <Outlet />
    );
};