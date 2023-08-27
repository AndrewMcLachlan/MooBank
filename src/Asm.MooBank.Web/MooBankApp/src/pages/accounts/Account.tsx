import { useIdParams } from "@andrewmclachlan/mooapp";
import { AccountProvider } from "components";
import { Navigate, Outlet, useLocation, useMatch } from "react-router-dom";
import { useAccount } from "services";

export const Account = () => {

    const id = useIdParams();

    const account = useAccount(id);

    const match = useMatch("/accounts/:id");

    if (match)
    {
        return <Navigate to={`/accounts/${id}/transactions`} replace />
    }

    return (
        <AccountProvider account={account.data}>
            <Outlet />
        </AccountProvider>
    );
}