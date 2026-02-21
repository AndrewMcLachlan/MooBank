import React from "react";
import { createFileRoute } from "@tanstack/react-router";

import { AccountList } from "../../components";
import { Page } from "@andrewmclachlan/moo-app";
import { IconLinkButton } from "@andrewmclachlan/moo-ds";

export const Route = createFileRoute("/accounts/")({
    component: Accounts,
});

function Accounts() {
    return (
        <Page title="Accounts" breadcrumbs={[{ text: "Accounts", route: "/accounts" }]} actions={
            [
                <IconLinkButton key="create-account" variant="primary" to="/accounts/create" icon="plus">Add Account</IconLinkButton>,
                <IconLinkButton key="create-stock" variant="primary" to="/shares/create" icon="plus">Add Shares</IconLinkButton>,
                <IconLinkButton key="create-asset" variant="primary" to="/assets/create" icon="plus" >Add Asset</IconLinkButton>,
            ]}>
            <AccountList />
        </Page>
    );
}
