import React from "react";

import { AccountList } from "../../components";
import { IconLinkButton, Page } from "@andrewmclachlan/mooapp";

export const Accounts: React.FC = () => (
    <Page title="Accounts" breadcrumbs={[{ text: "Accounts", route: "/accounts" }]} actions={
        [
            <IconLinkButton key="create-account" variant="primary" to="/accounts/create" icon="plus">Add Account</IconLinkButton>,
            <IconLinkButton key="create-stock" variant="primary" className="btn btn-primary" to="/shares/create" icon="plus">Add Shares</IconLinkButton>,
            <IconLinkButton key="create-asset" variant="primary" className="btn btn-primary" to="/assets/create" icon="plus" >Add Asset</IconLinkButton>,
        ]}>
        <AccountList />
    </Page>
);
