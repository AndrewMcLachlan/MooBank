import { Button, Col, ComboBox, Form, Input, Modal, Row } from "@andrewmclachlan/moo-ds";
import { useForm, useWatch } from "react-hook-form";
import type { AccountScopeMode, ForecastPlan } from "api/types.gen";
import { useUpdateForecastPlan } from "../-hooks/useUpdateForecastPlan";
import { useAccounts } from "hooks/useAccounts";

interface ForecastSettingsModalProps {
    plan?: ForecastPlan;
    show: boolean;
    onHide: () => void;
}

interface ForecastSettingsFormValues {
    name: string;
    startDate: string;
    endDate: string;
    accountScopeMode: AccountScopeMode;
    accountIds: string[];
    lookbackMonths: number;
}

const toFormValues = (plan?: ForecastPlan): ForecastSettingsFormValues => ({
    name: plan?.name ?? "",
    startDate: plan?.startDate ?? "",
    endDate: plan?.endDate ?? "",
    accountScopeMode: plan?.accountScopeMode,
    accountIds: plan?.accountIds ?? [],
    lookbackMonths: plan?.outgoingStrategy?.lookbackMonths ?? 24,
});

export const ForecastSettingsModal: React.FC<ForecastSettingsModalProps> = ({ plan, show, onHide }) => {

    const { data: accounts } = useAccounts();
    const { update, isPending } = useUpdateForecastPlan();

    const form = useForm<ForecastSettingsFormValues>({
        values: toFormValues(plan),
        resetOptions: { keepDirtyValues: true },
    });

    const accountScopeMode = useWatch({ control: form.control, name: "accountScopeMode" });
    const selectedAccountIds = useWatch({ control: form.control, name: "accountIds" });

    if (!plan) return null;

    const handleSave = (data: ForecastSettingsFormValues) => {
        update(plan.id, {
            name: data.name,
            startDate: data.startDate,
            endDate: data.endDate,
            accountScopeMode: data.accountScopeMode,
            accountIds: data.accountScopeMode === "SelectedAccounts" ? data.accountIds : [],
            outgoingStrategy: {
                ...plan.outgoingStrategy,
                lookbackMonths: Number(data.lookbackMonths) || 24,
            }
        });
        onHide();
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
                    <Form.Group groupId="lookbackMonths">
                        <Form.Label>History Used (months)</Form.Label>
                        <Form.Input type="number" min={1} max={60} />
                        <div className="field-hint">How far back spending is studied to work out how it moves with income.</div>
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
                        {accountScopeMode === "SelectedAccounts" && (
                            <ComboBox
                                multiSelect
                                clearable
                                placeholder="Select accounts..."
                                items={accounts ?? []}
                                selectedItems={(accounts ?? []).filter(a => selectedAccountIds.includes(a.id))}
                                labelField={a => a?.name}
                                valueField={a => a?.id}
                                onChange={(items) => form.setValue("accountIds", items.map(a => a.id), { shouldDirty: true })}
                            />
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
