import { Page, PageProps } from "@andrewmclachlan/mooapp";
import { Hierarchy } from "assets";
import { PropsWithChildren } from "react";

export const TagsPage: React.FC<PropsWithChildren<Pick<PageProps, "className">>> = ({ children, ...props }) => (
    <Page title="Tags" breadcrumbs={[{text: "Tags", route: "/tags"}]} navItems={[{ text: "Visualiser", route: "/tags/visualiser", image: <Hierarchy/> }]} {...props}>
        {children}
    </Page>
);