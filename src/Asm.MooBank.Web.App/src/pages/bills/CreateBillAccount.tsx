import { Page } from "@andrewmclachlan/moo-app";
import { BillAccountForm } from "./BillAccountForm";

export const CreateBillAccount: React.FC = () => {

    return (
        <Page title="Create Utility Account" breadcrumbs={[{ text: "Utilities", route: "/bills" }, { text: "Create Account", route: "/bills/accounts/create" }]}>
            <BillAccountForm />
        </Page>
    );
};

CreateBillAccount.displayName = "CreateBillAccount";
