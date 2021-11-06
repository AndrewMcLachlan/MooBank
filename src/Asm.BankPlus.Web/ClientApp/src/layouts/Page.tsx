import React from "react";
import { usePageTitle } from "../hooks";

import { PageContent, PageContentComponent } from "./PageContent";
import { PageHeader, PageHeaderComponent } from "./PageHeader";

export type PageComponent = React.FC<PageProps> & {
    Content: PageContentComponent;
    Header: PageHeaderComponent;
}

const Page: PageComponent = (props) => {

    usePageTitle(props.title);

    return (
        <article>
            {props.children}
        </article>
    )
}

Page.Content = PageContent;
Page.Header = PageHeader;

export { Page };

export interface PageProps {
    title?: string,
}