import { createFileRoute } from "@tanstack/react-router";
import { Page } from "@andrewmclachlan/moo-app";
import { AccountForm } from "./-components/AccountForm";

export const Route = createFileRoute("/accounts/create")({
    component: CreateAccount,
});

function CreateAccount() {
    return (
        <Page title="Create Account" breadcrumbs={[{ text: "Accounts", route: "/accounts" }, { text: "Create Account", route: "/accounts/create" }]}>
            <AccountForm />
        </Page>
    );
}
