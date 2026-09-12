import React from "react";
import { createFileRoute } from "@tanstack/react-router";

import { AccountList } from "../../components";
import { Page } from "@andrewmclachlan/moo-app";

export const Route = createFileRoute("/accounts/")({
    component: Accounts,
});

function Accounts() {
    return (
        <Page title="Accounts" breadcrumbs={[{ text: "Accounts", route: "/accounts" }]} actions={[
                { id: "create-account", label: "Add Account", icon: "plus", variant: "primary", to: "/accounts/create" },
                { id: "create-stock", label: "Add Shares", icon: "plus", variant: "primary", to: "/shares/create" },
                { id: "create-asset", label: "Add Asset", icon: "plus", variant: "primary", to: "/assets/create" },
            ]}>
            <AccountList />
        </Page>
    );
}
