import { createFileRoute, Outlet } from "@tanstack/react-router";
import { AccountProvider, TransactionListProvider } from "../../../../components";
import { useVirtualInstrument } from "../../../../services";

function VirtualAccountLayout() {
    const { id, virtualId } = Route.useParams();
    const account = useVirtualInstrument(id, virtualId);

    return (
        <AccountProvider account={account.data}>
            <TransactionListProvider>
                <Outlet />
            </TransactionListProvider>
        </AccountProvider>
    );
}

export const Route = createFileRoute("/accounts/$id/virtual/$virtualId")({
    component: VirtualAccountLayout,
});
