import { useEffect } from "react";
import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { useAccount } from "../../-hooks/useAccount";
import { getDefaultReportRoute } from "./-utils/reports";

export const Route = createFileRoute("/accounts/$id/reports/")({
    component: ReportsIndex,
});

function ReportsIndex() {
    const { id } = Route.useParams();
    const account = useAccount(id);
    const navigate = useNavigate();

    useEffect(() => {
        if (!account.data) return;
        navigate({
            to: getDefaultReportRoute(account.data.id, account.data.availableReports),
            replace: true,
        });
    }, [account.data, navigate]);

    return null;
}
