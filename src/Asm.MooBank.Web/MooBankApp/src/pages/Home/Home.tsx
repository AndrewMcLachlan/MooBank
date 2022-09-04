import React from "react";

import { AccountList } from "../../components";
import { Page } from "../../layouts";

export const Home: React.FC = () => (
    <Page title="Home">
        <Page.Header title="Accounts" hidebreadcrumb />
        <Page.Content>
            <AccountList />
        </Page.Content>
    </Page>
);
