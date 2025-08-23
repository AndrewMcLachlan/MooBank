import { useIdParams } from "@andrewmclachlan/moo-app";
import { StockHoldingProvider } from "./StockHoldingProvider";
import { Navigate, Outlet, useMatch } from "react-router";
import { useStockHolding } from "services";

export const StockHolding = () => {

    const id = useIdParams();

    const account = useStockHolding(id);

    const match = useMatch("/shares/:id");

    if (match) {
        return <Navigate to={`/shares/${id}/transactions`} replace />
    }

    return (
        <StockHoldingProvider stockHolding={account.data}>
            <Outlet />
        </StockHoldingProvider>
    );
}
