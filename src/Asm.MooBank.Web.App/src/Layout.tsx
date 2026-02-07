import "./App.css";
import * as Icons from "@andrewmclachlan/moo-icons";

import React from "react";
import { Link } from "react-router";

import { MooAppLayout } from "@andrewmclachlan/moo-app";
import { Icon, NavItem } from "@andrewmclachlan/moo-ds";
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
            header={{ menu: menu, userMenu: userMenu }}
            sidebar={{ navItems: sideMenu }}
        />
    );
};

export default Layout;

const userMenu: NavItem[] = [
    {
        text: "Profile",
        image: <Icon icon={Icons.User} />,
        route: "/profile",
    },
    {
        text: "My Family",
        image: <Icon icon={Icons.Users} />,
        route: "/family",
    }
];

const sideMenu = [
    {
        text: "Dashboard",
        image: <Icons.Dashboard />,
        route: "/"
    },
    {
        text: "Accounts",
        image: <Icons.PiggyBank />,
        route: "/accounts"
    },
    {
        text: "Bills",
        image: <Icons.TwoCoins />,
        route: "/bills"
    },
    {
        text: "Budget",
        image: <Icons.Budget />,
        route: "/budget"
    },
    {
        text: "Forecast",
        image: <Icons.Trendline />,
        route: "/forecast"
    },
    {
        text: "Groups",
        image: <Icons.Stack />,
        route: "/groups"
    },
    {
        text: "Tags",
        image: <Icons.Tags />,
        route: "/tags"
    },
];
