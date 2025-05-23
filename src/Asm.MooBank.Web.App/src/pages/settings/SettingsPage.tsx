import React, { PropsWithChildren, ReactNode } from "react";

import { NavItem, NavItemDivider, Page, PageProps } from "@andrewmclachlan/mooapp";

import { Sliders, Users } from "@andrewmclachlan/mooicons";

export const SettingsPage: React.FC<PropsWithChildren<SettingsPageProps>> = ({ children, breadcrumbs = [], ...props }) => {

    return (
        <Page title={props.title} actions={props.actions} navItems={getMenuItems(props.navItems ?? [])} breadcrumbs={[{ text: "Settings", route: "/settings" }, ...breadcrumbs]}>
            {children}
        </Page>
    )
}

const getMenuItems = (navItems: (ReactNode | NavItem)[]) => {

    const items: (NavItem | ReactNode)[] = [
        { route: `/settings/institutions`, text: "Institutions", image: <Sliders />  },
        { route: `/settings/families`, text: "Families", image: <Users /> }
    ];

    if (navItems.length > 0) {
        items.push(<NavItemDivider />);
    }

    return items.concat(navItems);
}

export interface SettingsPageProps extends PageProps {
}
