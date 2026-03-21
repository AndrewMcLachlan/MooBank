import React from "react";
import { Page } from "@andrewmclachlan/moo-app";
import type { PageProps } from "@andrewmclachlan/moo-app";
import { Hierarchy } from "@andrewmclachlan/moo-icons";
import type { PropsWithChildren } from "react";

export const TagsPage: React.FC<PropsWithChildren<Pick<PageProps, "className">>> = ({ children, ...props }) => (
    <Page title="Tags" breadcrumbs={[{text: "Tags", route: "/tags"}]} navItems={[{ text: "Visualiser", route: "/tags/visualiser", image: <Hierarchy/> }]} {...props}>
        {children}
    </Page>
);
