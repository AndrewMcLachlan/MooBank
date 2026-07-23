import { Section, SectionForm } from "@andrewmclachlan/moo-ds";
import { format, parseISO } from "date-fns";
import { useState } from "react";
import { Button, Col, Form, Input, OverlayTrigger, Popover, Row } from "@andrewmclachlan/moo-ds";
import { useForm, useWatch } from "react-hook-form";
import type { AccountScopeMode, ForecastPlan, RegressionDiagnostics } from "api/types.gen";
import { useUpdateForecastPlan } from "../-hooks/useUpdateForecastPlan";
import { useAccounts } from "hooks/useAccounts";
import { Amount, CurrencyInput } from "components";
import { formatCurrency } from "utils/currency";

interface ForecastSettingsProps {
    plan?: ForecastPlan;
    monthlyExpenses?: number;
    regression?: RegressionDiagnostics;
    currencyCode: string;
}

interface ForecastSettingsFormValues {
    name: string;
    startDate: string;
    endDate: string;
    monthlyIncome: number;
    accountScopeMode: AccountScopeMode;
    accountIds: string[];
    outgoingMode: string;
}

const toFormValues = (plan?: ForecastPlan): ForecastSettingsFormValues => ({
    name: plan?.name ?? "",
    startDate: plan?.startDate ?? "",
    endDate: plan?.endDate ?? "",
    monthlyIncome: plan?.incomeStrategy?.manualRecurring?.amount ?? 0,
    accountScopeMode: plan?.accountScopeMode,
    accountIds: plan?.accountIds ?? [],
    outgoingMode: plan?.outgoingStrategy?.mode ?? "HistoricalAverageByTag",
});

export const ForecastSettings: React.FC<ForecastSettingsProps> = ({ plan, monthlyExpenses, regression, currencyCode }) => {
    const [isEditing, setIsEditing] = useState(false);

    const { data: accounts } = useAccounts();
    const { update, isPending } = useUpdateForecastPlan();

    const form = useForm<ForecastSettingsFormValues>({
        values: toFormValues(plan),
        resetOptions: { keepDirtyValues: true },
    });

    const accountScopeMode = useWatch({ control: form.control, name: "accountScopeMode" });
    const selectedAccountIds = useWatch({ control: form.control, name: "accountIds" });
    const outgoingMode = useWatch({ control: form.control, name: "outgoingMode" });

    const handleSave = (data: ForecastSettingsFormValues) => {
        update(plan.id, {
            name: data.name,
            startDate: data.startDate,
            endDate: data.endDate,
            accountScopeMode: data.accountScopeMode,
            accountIds: data.accountScopeMode === "SelectedAccounts" ? data.accountIds : [],
            incomeStrategy: {
                ...plan.incomeStrategy,
                manualRecurring: {
                    ...plan.incomeStrategy?.manualRecurring,
                    amount: Number(data.monthlyIncome) || 0,
                    frequency: "Monthly"
                }
            },
            outgoingStrategy: {
                ...plan.outgoingStrategy,
                mode: data.outgoingMode,
            }
        });
        setIsEditing(false);
    };

    const handleAccountToggle = (accountId: string) => {
        const current = form.getValues("accountIds");
        form.setValue(
            "accountIds",
            current.includes(accountId)
                ? current.filter(id => id !== accountId)
                : [...current, accountId],
            { shouldDirty: true }
        );
    };

    const handleCancel = () => {
        form.reset(toFormValues(plan));
        setIsEditing(false);
    };

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

    if (!isEditing) {
        return (
            <Section header="Forecast Settings">
                <Row>
                    <Col md={2}>
                        <div className="settings-item">
                            <div className="settings-label">Plan Name</div>
                            <div className="settings-value">{plan?.name}</div>
                        </div>
                    </Col>
                    <Col md={2}>
                        <div className="settings-item">
                            <div className="settings-label">Period</div>
                            <div className="settings-value">
                                {plan && (`${format(parseISO(plan.startDate), "MMM yyyy")} - ${format(parseISO(plan.endDate), "MMM yyyy")}`)}
                            </div>
                        </div>
                    </Col>
                    <Col md={2}>
                        <div className="settings-item">
                            <div className="settings-label">Monthly Income</div>
                            <div className="settings-value">
                                <Amount amount={plan?.incomeStrategy?.manualRecurring?.amount ?? 0} currencyCode={currencyCode} minus />
                            </div>
                        </div>
                    </Col>
                    <Col md={2}>
                        <div className="settings-item">
                            <div className="settings-label">Monthly Expenses</div>
                            <div className="settings-value">
                                <Amount amount={monthlyExpenses ?? 0} currencyCode={currencyCode} minus />
                            </div>
                            <div className="settings-sublabel">
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
                    </Col>
                    <Col md={2}>
                        <div className="settings-item">
                            <div className="settings-label">Accounts</div>
                            <div className="settings-value">{getAccountsDisplay()}</div>
                        </div>
                    </Col>
                    <Col md={2} className="settings-actions">
                        <Button variant="outline-primary" size="sm" onClick={() => {
                            form.reset(toFormValues(plan));
                            setIsEditing(true);
                        }}>
                            Edit Settings
                        </Button>
                    </Col>
                </Row>
            </Section>
        );
    }

    return (
        <SectionForm form={form} onSubmit={handleSave} header="Forecast Settings">
            <Row className="g-3">
                <Col md={3}>
                    <Form.Group groupId="name">
                        <Form.Label>Plan Name</Form.Label>
                        <Form.Input type="text" />
                    </Form.Group>
                </Col>
                <Col md={2}>
                    <Form.Group groupId="startDate">
                        <Form.Label>Start Date</Form.Label>
                        <Form.Input type="date" />
                    </Form.Group>
                </Col>
                <Col md={2}>
                    <Form.Group groupId="endDate">
                        <Form.Label>End Date</Form.Label>
                        <Form.Input type="date" />
                    </Form.Group>
                </Col>
                <Col md={2}>
                    <Form.Group groupId="monthlyIncome">
                        <Form.Label>Monthly Income</Form.Label>
                        <CurrencyInput currency={currencyCode} min={0} />
                    </Form.Group>
                </Col>
                <Col md={3}>
                    <Form.Label>Expense Calculation</Form.Label>
                    <div>
                        <Input.Check
                            type="radio"
                            id="outgoing-historical"
                            name="outgoingMode"
                            label="Historical average"
                            checked={outgoingMode === "HistoricalAverageByTag"}
                            onChange={() => form.setValue("outgoingMode", "HistoricalAverageByTag", { shouldDirty: true })}
                            inline
                        />
                        <Input.Check
                            type="radio"
                            id="outgoing-correlated"
                            name="outgoingMode"
                            label="Income-correlated"
                            checked={outgoingMode === "IncomeCorrelated"}
                            onChange={() => form.setValue("outgoingMode", "IncomeCorrelated", { shouldDirty: true })}
                            inline
                        />
                    </div>
                </Col>
            </Row>
            <Row className="g-3 mt-2">
                <Col md={9} />
                <Col md={3} className="settings-actions">
                    <Button type="submit" variant="primary" size="sm" disabled={isPending}>
                        {isPending ? "Saving..." : "Save"}
                    </Button>
                    <Button variant="outline-secondary" size="sm" onClick={handleCancel}>
                        Cancel
                    </Button>
                </Col>
            </Row>
            <Row className="g-3 mt-2">
                <Col md={12}>
                    <Form.Label>Accounts</Form.Label>
                    <div className="mb-2">
                        <Input.Check
                            type="radio"
                            id="scope-all"
                            name="accountScope"
                            label="Use all accounts"
                            checked={accountScopeMode === "AllAccounts"}
                            onChange={() => form.setValue("accountScopeMode", "AllAccounts", { shouldDirty: true })}
                            inline
                        />
                        <Input.Check
                            type="radio"
                            id="scope-selected"
                            name="accountScope"
                            label="Select specific accounts"
                            checked={accountScopeMode === "SelectedAccounts"}
                            onChange={() => form.setValue("accountScopeMode", "SelectedAccounts", { shouldDirty: true })}
                            inline
                        />
                    </div>
                    {accountScopeMode === "SelectedAccounts" && accounts && (
                        <div className="border rounded p-2" style={{ maxHeight: "200px", overflowY: "auto" }}>
                            {accounts.map(account => (
                                <Input.Check
                                    key={account.id}
                                    type="checkbox"
                                    id={`account-${account.id}`}
                                    label={account.name}
                                    checked={selectedAccountIds.includes(account.id)}
                                    onChange={() => handleAccountToggle(account.id)}
                                />
                            ))}
                        </div>
                    )}
                </Col>
            </Row>
        </SectionForm>
    );
};
