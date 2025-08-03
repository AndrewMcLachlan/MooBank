import { AccountProvider, TransactionListProvider } from "components";
import { Navigate, Outlet, useMatch, useParams } from "react-router";
import { useVirtualAccount } from "services";

export const VirtualAccount = () => {

    const { id, virtualId } = useParams<any>();

    const account = useVirtualAccount(id, virtualId);

    const match = useMatch("/accounts/:id/virtual/:virtualId");

    if (match) {
        return <Navigate to={`/accounts/${id}/virtual/${virtualId}/transactions`} replace />
    }

    return (
        <AccountProvider account={account.data}>
            <TransactionListProvider>
                <Outlet />
            </TransactionListProvider>
        </AccountProvider>
    );
}
