import { Section } from "@andrewmclachlan/moo-ds";
import { format, parseISO } from "date-fns";
import { useState } from "react";
import { Button, Col, Form, Input, Row } from "@andrewmclachlan/moo-ds";
import type { AccountScopeMode, ForecastPlan } from "api/types.gen";
import { useUpdateForecastPlan } from "services/ForecastService";
import { useAccounts } from "services/AccountService";

interface ForecastSettingsProps {
    plan?: ForecastPlan;
    monthlyExpenses?: number;
}

export const ForecastSettings: React.FC<ForecastSettingsProps> = ({ plan, monthlyExpenses }) => {
    const [isEditing, setIsEditing] = useState(false);
    const [name, setName] = useState(plan?.name);
    const [startDate, setStartDate] = useState(plan?.startDate);
    const [endDate, setEndDate] = useState(plan?.endDate);
    const [monthlyIncome, setMonthlyIncome] = useState(plan?.incomeStrategy?.manualRecurring?.amount ?? 0);
    const [accountScopeMode, setAccountScopeMode] = useState<AccountScopeMode>(plan?.accountScopeMode);
    const [selectedAccountIds, setSelectedAccountIds] = useState<string[]>(plan?.accountIds ?? []);

    const { data: accounts } = useAccounts();
    const { update, isPending } = useUpdateForecastPlan();

    const handleSave = () => {
        update(plan.id, {
            name,
            startDate,
            endDate,
            accountScopeMode,
            accountIds: accountScopeMode === "SelectedAccounts" ? selectedAccountIds : [],
            incomeStrategy: {
                ...plan.incomeStrategy,
                manualRecurring: {
                    ...plan.incomeStrategy?.manualRecurring,
                    amount: monthlyIncome,
                    frequency: "Monthly"
                }
            }
        });
        setIsEditing(false);
    };

    const handleAccountToggle = (accountId: string) => {
        setSelectedAccountIds(prev =>
            prev.includes(accountId)
                ? prev.filter(id => id !== accountId)
                : [...prev, accountId]
        );
    };

    const handleCancel = () => {
        setName(plan.name);
        setStartDate(plan.startDate);
        setEndDate(plan.endDate);
        setMonthlyIncome(plan.incomeStrategy?.manualRecurring?.amount ?? 0);
        setAccountScopeMode(plan.accountScopeMode);
        setSelectedAccountIds(plan.accountIds ?? []);
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
                                ${(plan?.incomeStrategy?.manualRecurring?.amount ?? 0).toLocaleString(undefined, { minimumFractionDigits: 2 })}
                            </div>
                        </div>
                    </Col>
                    <Col md={2}>
                        <div className="settings-item">
                            <div className="settings-label">Monthly Expenses</div>
                            <div className="settings-value">
                                ${(monthlyExpenses ?? 0).toLocaleString(undefined, { minimumFractionDigits: 2 })}
                            </div>
                            <div className="settings-sublabel text-muted" style={{ fontSize: "0.75rem" }}>
                                (calculated from history)
                            </div>
                        </div>
                    </Col>
                    <Col md={2}>
                        <div className="settings-item">
                            <div className="settings-label">Accounts</div>
                            <div className="settings-value">{getAccountsDisplay()}</div>
                        </div>
                    </Col>
                    <Col md={2} className="d-flex align-items-center">
                        <Button variant="outline-primary" size="sm" onClick={() => setIsEditing(true)}>
                            Edit Settings
                        </Button>
                    </Col>
                </Row>
            </Section>
        );
    }

    return (
        <Section header="Forecast Settings">
            <Row className="g-3">
                <Col md={3}>
                        <Form.Label>Plan Name</Form.Label>
                        <Input
                            type="text"
                            value={name}
                            onChange={(e) => setName(e.target.value)}
                        />
                </Col>
                <Col md={2}>
                        <Form.Label>Start Date</Form.Label>
                        <Input
                            type="date"
                            value={startDate}
                            onChange={(e) => setStartDate(e.target.value)}
                        />
                </Col>
                <Col md={2}>
                        <Form.Label>End Date</Form.Label>
                        <Input
                            type="date"
                            value={endDate}
                            onChange={(e) => setEndDate(e.target.value)}
                        />
                </Col>
                <Col md={2}>
                        <Form.Label>Monthly Income</Form.Label>
                        <Input
                            type="number"
                            min={0}
                            step={0.01}
                            value={monthlyIncome}
                            onChange={(e) => setMonthlyIncome(parseFloat(e.target.value) || 0)}
                        />
                </Col>
                <Col md={3} className="d-flex align-items-end gap-2">
                    <Button variant="primary" size="sm" onClick={handleSave} disabled={isPending}>
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
                            onChange={() => setAccountScopeMode("AllAccounts")}
                            inline
                        />
                        <Input.Check
                            type="radio"
                            id="scope-selected"
                            name="accountScope"
                            label="Select specific accounts"
                            checked={accountScopeMode === "SelectedAccounts"}
                            onChange={() => setAccountScopeMode("SelectedAccounts")}
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
        </Section>
    );
};
