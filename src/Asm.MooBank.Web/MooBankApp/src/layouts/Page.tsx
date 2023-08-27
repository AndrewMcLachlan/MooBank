import React, { PropsWithChildren } from "react";

import { PageContent, PageContentComponent } from "./PageContent";
import { PageHeader, PageHeaderComponent, PageHeaderProps } from "./PageHeader";

export type PageComponent = React.FC<PropsWithChildren<PageProps>> & {
    Content: PageContentComponent;
    Header: PageHeaderComponent;
}

const Page: PageComponent = ({ children, ...props }) => {

    return (<>
        {children}
    </>);
}

Page.Content = PageContent;
Page.Header = PageHeader;

export { Page };

export interface PageProps extends PageHeaderProps {
    title: string,
}