import React, { PropsWithChildren } from "react";
import { usePageTitle } from "../hooks";

import { PageContent, PageContentComponent } from "./PageContent";
import { PageHeader, PageHeaderComponent } from "./PageHeader";

export type PageComponent = React.FC<PropsWithChildren<PageProps>> & {
    Content: PageContentComponent;
    Header: PageHeaderComponent;
}

const Page: PageComponent = (props) => {

    usePageTitle(props.title);

    return (<>{props.children}</>);
}

Page.Content = PageContent;
Page.Header = PageHeader;

export { Page };

export interface PageProps {
    title?: string,
}