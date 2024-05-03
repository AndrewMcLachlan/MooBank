import React, { PropsWithChildren, ReactNode } from "react";

import { NavItem, NavItemDivider, Page, PageProps } from "@andrewmclachlan/mooapp";
import { Asset } from "models";
import { useAsset } from "./AssetProvider";

import { Sliders, Transaction } from "assets";

export const AssetPage: React.FC<PropsWithChildren<AccountPageProps>> = ({ children, breadcrumbs =[], ...props }) => {

    const asset = useAsset();

    if (!asset) return null;

    return (
        <Page title={`${asset.name}${props.title && " : "}${props.title}`} actions={props.actions} navItems={getMenuItems(asset, props.navItems ?? [])} breadcrumbs={[{ text: "Accounts", route: "/accounts" }, { text: asset.name, route: `/shares/${asset.id}` }, ...breadcrumbs]}>
            {children}
        </Page>
    )
}

const getMenuItems = (asset: Asset, navItems: (NavItem | ReactNode)[]) => {

    if (!asset) return [];

    const items: (NavItem | ReactNode)[] = [
        { route: `/assets/${asset.id}/manage`, text: "Manage", image: <Sliders /> }
    ];

    if (navItems.length > 0) {
        items.push(<NavItemDivider />);
    }

    return items.concat(navItems);
}

export interface AccountPageProps extends PageProps {
}
