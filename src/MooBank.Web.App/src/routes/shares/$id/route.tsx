import { createFileRoute, Outlet } from "@tanstack/react-router";
import { StockHoldingProvider } from "../-components/StockHoldingProvider";
import { useStockHolding } from "../-hooks/useStockHolding";

function StockHoldingLayout() {
    const { id } = Route.useParams();
    const account = useStockHolding(id);

    return (
        <StockHoldingProvider stockHolding={account.data}>
            <Outlet />
        </StockHoldingProvider>
    );
}

export const Route = createFileRoute("/shares/$id")({
    component: StockHoldingLayout,
});
