import "./App.css";
import * as Icons from "@andrewmclachlan/mooicons";

import React from "react";
import { Link } from "react-router";

import { Icon, MooAppLayout, NavItem } from "@andrewmclachlan/mooapp";
import { useIsAuthenticated } from "@azure/msal-react";
import { useHasRole } from "hooks";

const App: React.FC = () => {

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

export default App;

const userMenu: NavItem[] = [
    {
        text: "Profile",
        image: <Icon icon="user" />,
        route: "/profile",
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
