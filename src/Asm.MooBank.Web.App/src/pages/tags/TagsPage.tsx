import { Page } from "@andrewmclachlan/mooapp";
import { Hierarchy } from "assets";
import { PropsWithChildren } from "react";

export const TagsPage: React.FC<PropsWithChildren<unknown>> = ({ children }) => (
    <Page title="Tags" breadcrumbs={[{text: "Tags", route: "/tags"}]} navItems={[{ text: "Visualiser", route: "/tags/visualiser", image: <Hierarchy/> }]}>
        {children}
    </Page>
);