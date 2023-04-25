import { Page } from "layouts";

export const TagsHeader = () => (
    <Page.Header title="Transaction Tags" breadcrumbs={[["Transaction Tags", "/settings"]]} menuItems={[{text: "Tags", route: "/settings/tags"}, {text: "Visualiser", route: "/settings/tags/visualiser"}]} />
);