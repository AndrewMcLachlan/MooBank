import { useIdParams } from "@andrewmclachlan/mooapp";
import { StockHoldingProvider } from "./StockHoldingProvider";
import { Navigate, Outlet, useMatch } from "react-router-dom";
import { useStockHolding } from "services";

export const StockHolding = () => {

    const id = useIdParams();

    const account = useStockHolding(id);

    const match = useMatch("/stock/:id");

    if (match) {
        return <Navigate to={`/stock/${id}/transactions`} replace />
    }

    return (
        <StockHoldingProvider stockHolding={account.data}>
            <Outlet />
        </StockHoldingProvider>
    );
}