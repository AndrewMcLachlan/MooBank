import { Section } from "@andrewmclachlan/moo-ds";
import { format, addYears } from "date-fns";
import { useEffect, useState } from "react";
import { Button, Form, Spinner } from "react-bootstrap";
import { useCreateForecastPlan, useForecastPlans, useForecastPlan, useRunForecast } from "services/ForecastService";
import { useAccounts } from "services/AccountService";
import { ForecastPage } from "./ForecastPage";
import { ForecastChart } from "./ForecastChart";
import { ForecastSummaryPanel } from "./ForecastSummaryPanel";
import { PlannedItemsTable } from "./PlannedItemsTable";
import { ForecastSettings } from "./ForecastSettings";

const formatDate = (date: Date) => format(date, "yyyy-MM-dd");

export const Forecast: React.FC = () => {
    const [planId, setPlanId] = useState<string | null>(null);
    const [selectedAccountIds, setSelectedAccountIds] = useState<string[]>([]);
    const [createError, setCreateError] = useState<string | null>(null);

    const { data: plans, isLoading: plansLoading } = useForecastPlans();
    const { data: accounts, isLoading: accountsLoading } = useAccounts();
    const { createAsync, isPending: isCreating } = useCreateForecastPlan();

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

    const handleAccountToggle = (accountId: string) => {
        setSelectedAccountIds(prev =>
            prev.includes(accountId)
                ? prev.filter(id => id !== accountId)
                : [...prev, accountId]
        );
    };

    const handleSelectAll = () => {
        if (accounts) {
            setSelectedAccountIds(accounts.map(a => a.id));
        }
    };

    const handleSelectNone = () => {
        setSelectedAccountIds([]);
    };

    const handleCreatePlan = async () => {
        const today = new Date();
        const startDate = new Date(today.getFullYear(), today.getMonth(), 1);
        const endDate = addYears(startDate, 2);

        try {
            const newPlan = await createAsync({
                name: "My Forecast",
                startDate: formatDate(startDate),
                endDate: formatDate(endDate),
                accountScopeMode: "SelectedAccounts",
                startingBalanceMode: "CalculatedCurrent",
                accountIds: selectedAccountIds,
                incomeStrategy: {
                    version: 1,
                    mode: "ManualRecurring",
                    manualRecurring: {
                        amount: 0,
                        frequency: "Monthly"
                    }
                },
                outgoingStrategy: {
                    version: 1,
                    mode: "HistoricalAverageByTag",
                    lookbackMonths: 12
                }
            });
            setPlanId(newPlan.id);
            setCreateError(null);
        } catch (error: any) {
            setCreateError(error?.message ?? "Failed to create forecast plan");
        }
    };

    // Loading state
    if (plansLoading || accountsLoading) {
        return (
            <ForecastPage>
                <Section>
                    <div className="text-center py-5">
                        <Spinner animation="border" role="status">
                            <span className="visually-hidden">Loading...</span>
                        </Spinner>
                        <p className="mt-3">Loading...</p>
                    </div>
                </Section>
            </ForecastPage>
        );
    }

    // No plan exists - show account selection screen
    if (plans && plans.length === 0) {
        return (
            <ForecastPage>
                <Section header="Create Forecast Plan">
                    <p className="mb-4">
                        Select the accounts you want to include in your forecast.
                        The forecast will use historical data from these accounts to calculate your income and expenses.
                    </p>

                    {createError && (
                        <div className="alert alert-danger mb-3">{createError}</div>
                    )}

                    <div className="mb-3">
                        <Button variant="link" size="sm" className="p-0 me-3" onClick={handleSelectAll}>
                            Select All
                        </Button>
                        <Button variant="link" size="sm" className="p-0" onClick={handleSelectNone}>
                            Select None
                        </Button>
                    </div>

                    <div className="border rounded p-3 mb-4" style={{ maxHeight: "300px", overflowY: "auto" }}>
                        {accounts?.map(account => (
                            <Form.Check
                                key={account.id}
                                type="checkbox"
                                id={`account-${account.id}`}
                                label={`${account.name} ($${account.currentBalance.toLocaleString(undefined, { minimumFractionDigits: 2 })})`}
                                checked={selectedAccountIds.includes(account.id)}
                                onChange={() => handleAccountToggle(account.id)}
                                className="mb-2"
                            />
                        ))}
                        {(!accounts || accounts.length === 0) && (
                            <p className="text-muted mb-0">No accounts found.</p>
                        )}
                    </div>

                    <Button
                        variant="primary"
                        onClick={handleCreatePlan}
                        disabled={isCreating || selectedAccountIds.length === 0}
                    >
                        {isCreating ? (
                            <>
                                <Spinner animation="border" size="sm" className="me-2" />
                                Creating...
                            </>
                        ) : (
                            `Create Forecast with ${selectedAccountIds.length} Account${selectedAccountIds.length !== 1 ? 's' : ''}`
                        )}
                    </Button>
                </Section>
            </ForecastPage>
        );
    }

    // Plan exists - show forecast
    return (
        <ForecastPage>
            {plan && (
                <>
                    <ForecastSettings plan={plan} monthlyExpenses={result?.summary.monthlyBaselineOutgoings} />

                    {result && (
                        <>
                            <ForecastSummaryPanel summary={result.summary} />
                            <div style={{ position: "relative" }}>
                                <ForecastChart months={result.months} />
                                {resultLoading && (
                                    <div style={{
                                        position: "absolute",
                                        top: 0,
                                        left: 0,
                                        right: 0,
                                        bottom: 0,
                                        backgroundColor: "rgba(255, 255, 255, 0.7)",
                                        display: "flex",
                                        alignItems: "center",
                                        justifyContent: "center",
                                        zIndex: 10
                                    }}>
                                        <Spinner animation="border" size="sm" /> <span className="ms-2">Updating...</span>
                                    </div>
                                )}
                            </div>
                        </>
                    )}

                    {resultLoading && !result && (
                        <Section>
                            <div className="text-center py-4">
                                <Spinner animation="border" size="sm" /> Calculating forecast...
                            </div>
                        </Section>
                    )}

                    <PlannedItemsTable planId={plan.id} items={plan.plannedItems} />
                </>
            )}

            {planLoading && (
                <Section>
                    <div className="text-center py-4">
                        <Spinner animation="border" size="sm" /> Loading plan details...
                    </div>
                </Section>
            )}
        </ForecastPage>
    );
};
