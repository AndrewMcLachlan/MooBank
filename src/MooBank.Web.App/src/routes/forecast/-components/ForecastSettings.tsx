import { Section, OverlayTrigger, Popover } from "@andrewmclachlan/moo-ds";
import { format, parseISO } from "date-fns";
import type { ForecastPlan, RegressionDiagnostics } from "api/types.gen";
import { useAccounts } from "hooks/useAccounts";
import { Amount } from "components";
import { formatCurrency } from "utils/currency";

interface ForecastSettingsProps {
    plan?: ForecastPlan;
    monthlyExpenses?: number;
    regression?: RegressionDiagnostics;
    currencyCode: string;
}

export const ForecastSettings: React.FC<ForecastSettingsProps> = ({ plan, monthlyExpenses, regression, currencyCode }) => {

    const { data: accounts } = useAccounts();

    const getAccountsDisplay = () => {
        if (plan?.accountScopeMode === "AllAccounts") {
            return "All Accounts";
        }
        if (!plan?.accountIds?.length) {
            return "No accounts selected";
        }
        const selectedNames = accounts?.filter(a => plan.accountIds.includes(a.id)).map(a => a.name) ?? [];
        return selectedNames.length > 2
            ? `${selectedNames.slice(0, 2).join(", ")} +${selectedNames.length - 2} more`
            : selectedNames.join(", ");
    };

    return (
        <Section header="Forecast Settings" className="forecast-settings-summary">
            <div className="settings-grid">
                <div className="setting">
                    <div className="setting-label">Period</div>
                    <div className="setting-value">
                        {plan && (`${format(parseISO(plan.startDate), "MMM yyyy")} - ${format(parseISO(plan.endDate), "MMM yyyy")}`)}
                    </div>
                </div>
                <div className="setting">
                    <div className="setting-label">Monthly Income</div>
                    <div className="setting-value">
                        <Amount amount={plan?.incomeStrategy?.manualRecurring?.amount ?? 0} currencyCode={currencyCode} minus />
                    </div>
                </div>
                <div className="setting">
                    <div className="setting-label">Monthly Expenses</div>
                    <div className="setting-value">
                        <Amount amount={monthlyExpenses ?? 0} currencyCode={currencyCode} minus />
                    </div>
                    <div className="setting-sub">
                        {plan?.outgoingStrategy?.mode === "IncomeCorrelated" && regression && !regression.fellBackToFlatAverage ? (
                            <OverlayTrigger placement="bottom" overlay={
                                <Popover id="regression-popover">
                                    <Popover.Body>
                                        <div className="regression-popover">
                                            <div>Fixed expenses: {formatCurrency(regression.fixedComponent)}/mo</div>
                                            <div>Variable rate: {(regression.variableComponent * 100).toLocaleString(undefined, { minimumFractionDigits: 1, maximumFractionDigits: 1 })}% of income</div>
                                            <div>Model fit: {(regression.rSquared * 100).toLocaleString(undefined, { minimumFractionDigits: 1, maximumFractionDigits: 1 })}% R²</div>
                                        </div>
                                    </Popover.Body>
                                </Popover>
                            }>
                                <span className="regression-hint">(average, income-correlated)</span>
                            </OverlayTrigger>
                        ) : plan?.outgoingStrategy?.mode === "IncomeCorrelated" ? "(income-correlated, using flat average)" : "(calculated from history)"}
                    </div>
                </div>
                <div className="setting">
                    <div className="setting-label">Accounts</div>
                    <div className="setting-value">{getAccountsDisplay()}</div>
                </div>
            </div>
        </Section>
    );
};
