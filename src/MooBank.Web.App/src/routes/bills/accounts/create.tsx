import { createFileRoute } from "@tanstack/react-router";
import { Page } from "@andrewmclachlan/moo-app";
import { BillAccountForm } from "../-components/BillAccountForm";

export const Route = createFileRoute("/bills/accounts/create")({
    component: CreateBillAccount,
});

function CreateBillAccount() {

    return (
        <Page title="Create Utility Account" breadcrumbs={[{ text: "Utilities", route: "/bills" }, { text: "Create Account", route: "/bills/accounts/create" }]}>
            <BillAccountForm />
        </Page>
    );
}
