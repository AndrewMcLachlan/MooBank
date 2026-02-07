import React from "react";
import { Page, PageProps } from "@andrewmclachlan/moo-app";
import { Hierarchy } from "@andrewmclachlan/moo-icons";
import { PropsWithChildren } from "react";

export const TagsPage: React.FC<PropsWithChildren<Pick<PageProps, "className">>> = ({ children, ...props }) => (
    <Page title="Tags" breadcrumbs={[{text: "Tags", route: "/tags"}]} navItems={[{ text: "Visualiser", route: "/tags/visualiser", image: <Hierarchy/> }]} {...props}>
        {children}
    </Page>
);
