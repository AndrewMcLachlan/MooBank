import { useIdParams } from "@andrewmclachlan/mooapp";
import { AccountProvider } from "components";
import { TransactionListProvider } from "components";
import { Navigate, Outlet, useMatch } from "react-router";
import { useAccount } from "services";

export const Account = () => {

    const id = useIdParams();

    const account = useAccount(id);

    const match = useMatch("/accounts/:id");

    if (match) {
        return <Navigate to={`/accounts/${id}/transactions${window.location.search}`} replace />
    }

    return (
        <AccountProvider account={account.data}>
            <TransactionListProvider>
                <Outlet />
            </TransactionListProvider>
        </AccountProvider>
    );
}
