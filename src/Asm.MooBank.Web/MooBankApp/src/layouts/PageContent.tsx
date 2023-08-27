import React from "react";

import { Section } from "@andrewmclachlan/mooapp";

export type PageContentComponent = React.FC<React.PropsWithChildren<PageContentProps>>;

export const PageContent: PageContentComponent = (props) => (
    <Section>
        {props.children}
    </Section>
)

export interface PageContentProps { }