import "./App.css";
import * as Icons from "@andrewmclachlan/moo-icons";

import React from "react";
import { Link } from "@tanstack/react-router";

import { MooAppLayout } from "@andrewmclachlan/moo-app";
import type { NavItem } from "@andrewmclachlan/moo-ds";
import { useIsAuthenticated } from "@azure/msal-react";
import { useHasRole } from "hooks";

const Layout: React.FC = () => {

    const isAuthenticated = useIsAuthenticated();

    const hasRole = useHasRole();

    if (!isAuthenticated) return null;

    const menu = hasRole("Admin") ?
        [(<Link key="settings" to="/settings" aria-label="Settings"><Icons.Cog /></Link>)] :
        [];

    return (
        <MooAppLayout
            header={{ menu: menu, userMenu: userMenu, showAppInfo: true }}
            sidebar={{ navItems: sideMenu }}
        />
    );
};

export default Layout;

const userMenu: NavItem[] = [
    {
        text: "Profile",
        icon: Icons.User,
        route: "/profile",
    },
    {
        text: "My Family",
        icon: Icons.Users,
        route: "/family",
    }
];

const sideMenu = [
    {
        text: "Dashboard",
        icon: Icons.Dashboard,
        route: "/"
    },
    {
        text: "Accounts",
        icon: Icons.PiggyBank,
        route: "/accounts"
    },
    {
        text: "Bills",
        icon: Icons.TwoCoins,
        route: "/bills"
    },
    {
        text: "Budget",
        icon: Icons.Budget,
        route: "/budget"
    },
    {
        text: "Planning",
        icon: Icons.Trendline,
        route: "/planning"
    },
    {
        text: "Groups",
        icon: Icons.Stack,
        route: "/groups"
    },
    {
        text: "Tags",
        icon: Icons.Tags,
        route: "/tags"
    },
];
