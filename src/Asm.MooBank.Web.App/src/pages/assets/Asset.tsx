import { useIdParams } from "@andrewmclachlan/moo-app";
import { AssetProvider } from "./AssetProvider";
import { Navigate, Outlet, useMatch } from "react-router";
import { useAsset } from "services";

export const Asset = () => {

    const id = useIdParams();

    const account = useAsset(id);

    const match = useMatch("/assets/:id");

    if (match) {
        return <Navigate to={`/assets/${id}/manage`} replace />
    }

    return (
        <AssetProvider asset={account.data}>
            <Outlet />
        </AssetProvider>
    );
}
