import { Button, Col, Form, Input, Modal, Row } from "@andrewmclachlan/moo-ds";
import { useForm, useWatch } from "react-hook-form";
import type { AccountScopeMode, ForecastPlan } from "api/types.gen";
import { useUpdateForecastPlan } from "../-hooks/useUpdateForecastPlan";
import { useAccounts } from "hooks/useAccounts";
import { CurrencyInput } from "components";

interface ForecastSettingsModalProps {
    plan?: ForecastPlan;
    currencyCode: string;
    show: boolean;
    onHide: () => void;
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

export const ForecastSettingsModal: React.FC<ForecastSettingsModalProps> = ({ plan, currencyCode, show, onHide }) => {

    const { data: accounts } = useAccounts();
    const { update, isPending } = useUpdateForecastPlan();

    const form = useForm<ForecastSettingsFormValues>({
        values: toFormValues(plan),
        resetOptions: { keepDirtyValues: true },
    });

    const accountScopeMode = useWatch({ control: form.control, name: "accountScopeMode" });
    const selectedAccountIds = useWatch({ control: form.control, name: "accountIds" });
    const outgoingMode = useWatch({ control: form.control, name: "outgoingMode" });

    if (!plan) return null;

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
        onHide();
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

    const handleHide = () => {
        form.reset(toFormValues(plan));
        onHide();
    };

    return (
        <Modal show={show} onHide={handleHide} size="lg" title="Edit Forecast Settings">
            <Form form={form} onSubmit={handleSave}>
                <Modal.Header closeButton>
                    <Modal.Title>Edit Forecast Settings</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    <Form.Group groupId="name">
                        <Form.Label>Plan Name</Form.Label>
                        <Form.Input type="text" />
                    </Form.Group>
                    <Row className="g-3">
                        <Col md={6}>
                            <Form.Group groupId="startDate">
                                <Form.Label>Start Date</Form.Label>
                                <Form.Input type="date" />
                            </Form.Group>
                        </Col>
                        <Col md={6}>
                            <Form.Group groupId="endDate">
                                <Form.Label>End Date</Form.Label>
                                <Form.Input type="date" />
                            </Form.Group>
                        </Col>
                    </Row>
                    <Form.Group groupId="monthlyIncome">
                        <Form.Label>Monthly Income</Form.Label>
                        <CurrencyInput currency={currencyCode} min={0} />
                    </Form.Group>
                    <Form.Group groupId="outgoingMode">
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
                    </Form.Group>
                    <Form.Group groupId="accountScopeMode">
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
                            <div className="account-checklist">
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
                    </Form.Group>
                </Modal.Body>
                <Modal.Footer>
                    <Button variant="outline-primary" onClick={handleHide}>Close</Button>
                    <Button type="submit" variant="primary" disabled={isPending}>Save</Button>
                </Modal.Footer>
            </Form>
        </Modal>
    );
};
