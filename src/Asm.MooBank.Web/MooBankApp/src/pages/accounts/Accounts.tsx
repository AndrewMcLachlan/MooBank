import React from "react";

import { AccountList } from "../../components";
import { Page } from "@andrewmclachlan/mooapp";

export const Accounts: React.FC = () => (
    <Page title="Home">
        <AccountList />
    </Page>
);
