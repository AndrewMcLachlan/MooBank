import { createFileRoute } from "@tanstack/react-router";

import { useIdParams } from "@andrewmclachlan/moo-app";

import { BillAccountForm } from "../../-components/BillAccountForm";
import { BillsPage } from "../../-components/BillsPage";
import { useBillAccount } from "../../-hooks/useBillAccount";

export const Route = createFileRoute("/bills/accounts/$id/edit")({
    component: EditBillAccount,
});

function EditBillAccount() {

    const id = useIdParams();

    const { data: billAccount } = useBillAccount(id);

    if (!billAccount) return null;

    return (
        <BillsPage
            title={billAccount.name}
            breadcrumbs={[{ text: "Accounts", route: "/bills/accounts" }, { text: billAccount.name, route: `/bills/accounts/${id}` }, { text: "Edit", route: `/bills/accounts/${id}/edit` }]}
        >
            <BillAccountForm key={billAccount.id} account={billAccount} />
        </BillsPage>
    );
}
