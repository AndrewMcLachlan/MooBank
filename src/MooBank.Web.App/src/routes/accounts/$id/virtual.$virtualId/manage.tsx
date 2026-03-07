import { createFileRoute } from "@tanstack/react-router";
import { ManageVirtualAccount } from "../../-components/ManageVirtualAccount";

export const Route = createFileRoute("/accounts/$id/virtual/$virtualId/manage")({
    component: ManageVirtualAccount,
});
