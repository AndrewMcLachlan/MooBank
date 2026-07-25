import { createFileRoute } from "@tanstack/react-router";
import { IconButton, SpinnerContainer } from "@andrewmclachlan/moo-ds";
import { Sliders } from "@andrewmclachlan/moo-icons";
import { useEffect, useState } from "react";
import { useForecastPlans } from "./-hooks/useForecastPlans";
import { useForecastPlan } from "./-hooks/useForecastPlan";
import { useForecastResult } from "./-hooks/useForecastResult";
import { useAccounts } from "hooks/useAccounts";
import { useUser } from "hooks/useUser";
import { ForecastPage } from "./-components/ForecastPage";
import { ForecastOutlook } from "./-components/ForecastOutlook";
import { PlannedItemsTable } from "./-components/PlannedItemsTable";
import { ForecastSettingsModal } from "./-components/ForecastSettingsModal";
import { CreateForecastPlan } from "./-components/CreateForecastPlan";

export const Route = createFileRoute("/forecast/")({
    component: Forecast,
});

function Forecast() {
    const [planId, setPlanId] = useState<string | null>(null);
    const [editOpen, setEditOpen] = useState(false);

    const { data: plans, isLoading: plansLoading } = useForecastPlans();
    const { data: accounts, isLoading: accountsLoading } = useAccounts();
    const { data: user } = useUser();

    // Set planId when plans are loaded and one exists
    useEffect(() => {
        if (plans && plans.length > 0 && !planId) {
            setPlanId(plans[0].id);
        }
    }, [plans, planId]);

    const { data: plan } = useForecastPlan(planId);
    const { data: result, isFetching: resultLoading } = useForecastResult(planId);

    // The forecast is denominated in the plan's currency, falling back to the user's preferred currency.
    const currencyCode = plan?.currencyCode ?? user?.currency ?? "AUD";

    // Loading state
    if (plansLoading || accountsLoading) {
        return (
            <ForecastPage>
                <SpinnerContainer />
            </ForecastPage>
        );
    }

    // No plan exists - show account selection screen
    if (plans && plans.length === 0 && accounts) {
        return (
            <ForecastPage>
                <CreateForecastPlan accounts={accounts} onPlanCreated={setPlanId} />
            </ForecastPage>
        );
    }

    const actions = plan ? [
        <IconButton badge key="edit-settings" variant="primary" icon={Sliders} onClick={() => setEditOpen(true)}>Edit Settings</IconButton>
    ] : [];

    return (
        <ForecastPage plan={plan} actions={actions}>
            <ForecastOutlook plan={plan} summary={result?.summary} months={result?.months ?? []} currencyCode={currencyCode} loading={resultLoading} />

            <PlannedItemsTable plan={plan} currencyCode={currencyCode} />

            <ForecastSettingsModal plan={plan} currencyCode={currencyCode} show={editOpen} onHide={() => setEditOpen(false)} />
        </ForecastPage>
    );
}
