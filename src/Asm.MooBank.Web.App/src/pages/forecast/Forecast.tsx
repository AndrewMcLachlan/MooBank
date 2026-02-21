import { Section, SpinnerContainer } from "@andrewmclachlan/moo-ds";
import { useEffect, useState } from "react";
import { Spinner } from "@andrewmclachlan/moo-ds";
import { useForecastPlans, useForecastPlan, useRunForecast } from "services/ForecastService";
import { useAccounts } from "services/AccountService";
import { ForecastPage } from "./ForecastPage";
import { ForecastChart } from "./ForecastChart";
import { ForecastSummaryPanel } from "./ForecastSummaryPanel";
import { PlannedItemsTable } from "./PlannedItemsTable";
import { ForecastSettings } from "./ForecastSettings";
import { CreateForecastPlan } from "./CreateForecastPlan";

export const Forecast: React.FC = () => {
    const [planId, setPlanId] = useState<string | null>(null);

    const { data: plans, isLoading: plansLoading } = useForecastPlans();
    const { data: accounts, isLoading: accountsLoading } = useAccounts();

    // Set planId when plans are loaded and one exists
    useEffect(() => {
        if (plans && plans.length > 0 && !planId) {
            setPlanId(plans[0].id);
        }
    }, [plans, planId]);

    const { data: plan, isLoading: planLoading } = useForecastPlan(planId);
    const { run: runForecast, result, isPending: resultLoading } = useRunForecast();

    // Run forecast when plan is loaded or updated
    useEffect(() => {
        if (planId && plan) {
            runForecast(planId);
        }
    }, [planId, plan?.updatedUtc]);

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
                    <ForecastSettings plan={plan} monthlyExpenses={result?.summary.monthlyBaselineOutgoings} regression={result?.summary.regression} />

                    <ForecastSummaryPanel summary={result?.summary} />
                    <div>
                        <ForecastChart months={result?.months ?? []} />
                        {resultLoading && (<SpinnerContainer />)}
                    </div>

                    <PlannedItemsTable plan={plan} />
                </>
            )}

        </ForecastPage>
    );
};
