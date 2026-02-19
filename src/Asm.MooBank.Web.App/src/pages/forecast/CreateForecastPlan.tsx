import { Section } from "@andrewmclachlan/moo-ds";
import { format, addYears } from "date-fns";
import { useState } from "react";
import { Button, Input, Spinner } from "@andrewmclachlan/moo-ds";
import { useCreateForecastPlan } from "services/ForecastService";
import type { LogicalAccount } from "api/types.gen";

const formatDate = (date: Date) => format(date, "yyyy-MM-dd");

interface CreateForecastPlanProps {
    accounts: LogicalAccount[];
    onPlanCreated: (planId: string) => void;
}

export const CreateForecastPlan: React.FC<CreateForecastPlanProps> = ({ accounts, onPlanCreated }) => {
    const [selectedAccountIds, setSelectedAccountIds] = useState<string[]>([]);
    const [createError, setCreateError] = useState<string | null>(null);

    const { createAsync, isPending: isCreating } = useCreateForecastPlan();

    const handleAccountToggle = (accountId: string) => {
        setSelectedAccountIds(prev =>
            prev.includes(accountId)
                ? prev.filter(id => id !== accountId)
                : [...prev, accountId]
        );
    };

    const handleSelectAll = () => {
        setSelectedAccountIds(accounts.map(a => a.id));
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
            onPlanCreated(newPlan.id);
            setCreateError(null);
        } catch (error: any) {
            setCreateError(error?.message ?? "Failed to create forecast plan");
        }
    };

    return (
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
                {accounts.map(account => (
                    <Input.Check
                        key={account.id}
                        type="checkbox"
                        id={`account-${account.id}`}
                        label={`${account.name} ($${account.currentBalance.toLocaleString(undefined, { minimumFractionDigits: 2 })})`}
                        checked={selectedAccountIds.includes(account.id)}
                        onChange={() => handleAccountToggle(account.id)}
                        className="mb-2"
                    />
                ))}
                {accounts.length === 0 && (
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
    );
};
