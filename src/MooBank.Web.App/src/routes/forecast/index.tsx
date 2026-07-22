import { createFileRoute } from "@tanstack/react-router";
import { SpinnerContainer } from "@andrewmclachlan/moo-ds";
import { useEffect, useState } from "react";
import { useForecastPlans } from "./-hooks/useForecastPlans";
import { useForecastPlan } from "./-hooks/useForecastPlan";
import { useForecastResult } from "./-hooks/useForecastResult";
import { useAccounts } from "hooks/useAccounts";
import { useUser } from "hooks/useUser";
import { ForecastPage } from "./-components/ForecastPage";
import { ForecastChart } from "./-components/ForecastChart";
import { ForecastSummaryPanel } from "./-components/ForecastSummaryPanel";
import { PlannedItemsTable } from "./-components/PlannedItemsTable";
import { ForecastSettings } from "./-components/ForecastSettings";
import { CreateForecastPlan } from "./-components/CreateForecastPlan";

export const Route = createFileRoute("/forecast/")({
    component: Forecast,
});

function Forecast() {
    const [planId, setPlanId] = useState<string | null>(null);

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

    return (
        <ForecastPage>
            {(
                <>
                    <ForecastSettings plan={plan} monthlyExpenses={result?.summary.monthlyBaselineOutgoings} regression={result?.summary.regression} currencyCode={currencyCode} />

                    <ForecastSummaryPanel summary={result?.summary} currencyCode={currencyCode} />
                    <div>
                        <ForecastChart months={result?.months ?? []} currencyCode={currencyCode} />
                        {resultLoading && (<SpinnerContainer />)}
                    </div>

                    <PlannedItemsTable plan={plan} currencyCode={currencyCode} />
                </>
            )}

        </ForecastPage>
    );
}
