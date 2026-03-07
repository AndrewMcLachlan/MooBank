import { createFileRoute, Outlet, redirect } from "@tanstack/react-router";
import { AssetProvider } from "../-components/AssetProvider";
import { useAsset } from "../-hooks/useAsset";

function AssetLayout() {
    const { id } = Route.useParams();
    const account = useAsset(id);

    return (
        <AssetProvider asset={account.data}>
            <Outlet />
        </AssetProvider>
    );
}

export const Route = createFileRoute("/assets/$id")({
    component: AssetLayout,
});
