import React from "react";
import type { PropsWithChildren, ReactNode } from "react";

import { Page } from "@andrewmclachlan/moo-app";
import type { PageProps } from "@andrewmclachlan/moo-app";
import { NavItemDivider } from "@andrewmclachlan/moo-ds";
import type { NavItem } from "@andrewmclachlan/moo-ds";
import type { Asset } from "api/types.gen";
import { useAsset } from "./AssetProvider";

import { Sliders } from "@andrewmclachlan/moo-icons";

export const AssetPage: React.FC<PropsWithChildren<AccountPageProps>> = ({ children, breadcrumbs =[], ...props }) => {

    const asset = useAsset();

    if (!asset) return null;

    return (
        <Page title={`${asset.name}${props.title && " : "}${props.title}`} actions={props.actions} navItems={getMenuItems(asset, props.navItems ?? [])} breadcrumbs={[{ text: "Accounts", route: "/accounts" }, { text: asset.name, route: `/assets/${asset.id}` }, ...breadcrumbs]}>
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
