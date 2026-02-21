import { createFileRoute, Outlet } from "@tanstack/react-router";
import { AccountProvider, TransactionListProvider } from "../../../components";
import { useAccount } from "../../../services";

function AccountLayout() {
    const { id } = Route.useParams();
    const account = useAccount(id);

    return (
        <AccountProvider account={account.data}>
            <TransactionListProvider>
                <Outlet />
            </TransactionListProvider>
        </AccountProvider>
    );
}

export const Route = createFileRoute("/accounts/$id")({
    component: AccountLayout,
});
