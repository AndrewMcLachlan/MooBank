import { Page } from "@andrewmclachlan/mooapp";
import { AccountForm } from "./AccountForm";

export const CreateAccount: React.FC = () => {

    return (
        <Page title="Create Account" breadcrumbs={[{ text: "Accounts", route: "/accounts" }, { text: "Create Account", route: "/accounts/create" }]}>
            <AccountForm />
        </Page>
    );
}

CreateAccount.displayName = "CreateAccount";
