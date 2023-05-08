import { Page } from "layouts";

export const TagsHeader = () => (
    <Page.Header title="Tags" breadcrumbs={[["Tags", "/settings"]]} menuItems={[{text: "Tags", route: "/settings/tags"}, {text: "Visualiser", route: "/settings/tags/visualiser"}]} />
);