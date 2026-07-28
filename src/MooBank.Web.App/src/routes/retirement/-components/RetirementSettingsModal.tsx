import { Button, ComboBox, Form, Modal } from "@andrewmclachlan/moo-ds";
import { useFieldArray, useForm, useWatch } from "react-hook-form";
import type { LogicalAccount, RetirementPlan, SimpleRetirementPlan } from "api/types.gen";
import { useUpdateRetirementPlan } from "../-hooks/useUpdateRetirementPlan";
import { useAccounts } from "hooks/useAccounts";
import { CurrencyInput } from "components";
import { defaultRetirementAge, fromPercent, toPercent } from "../-utils/retirementDefaults";

interface RetirementSettingsModalProps {
    plan?: RetirementPlan;
    currencyCode: string;
    show: boolean;
    onHide: () => void;
}

/**
 * The form holds rates as percentages because that is how people think about them; they are
 * converted back to fractions on save.
 */
interface RetirementSettingsFormValues {
    name: string;
    expectedReturnPercent: number;
    inflationPercent: number;
    superGuaranteePercent: number;
    contributionsTaxPercent: number;
    lifeExpectancy: number;
    members: {
        id?: string;
        name: string;
        dateOfBirth: string;
        currentIncome: number;
        retirementAge: number;
        instrumentIds: string[];
    }[];
}

const toFormValues = (plan?: RetirementPlan): RetirementSettingsFormValues => ({
    name: plan?.name ?? "",
    expectedReturnPercent: toPercent(plan?.expectedReturnRate),
    inflationPercent: toPercent(plan?.inflationRate),
    superGuaranteePercent: toPercent(plan?.superGuaranteeRate),
    contributionsTaxPercent: toPercent(plan?.contributionsTaxRate),
    lifeExpectancy: plan?.lifeExpectancy ?? 90,
    members: (plan?.members ?? []).map(m => ({
        id: m.id,
        name: m.name,
        dateOfBirth: m.dateOfBirth,
        currentIncome: m.currentIncome,
        retirementAge: m.retirementAge,
        instrumentIds: [...m.instrumentIds],
    })),
});

const toRequest = (data: RetirementSettingsFormValues): SimpleRetirementPlan => ({
    name: data.name,
    expectedReturnRate: fromPercent(data.expectedReturnPercent),
    inflationRate: fromPercent(data.inflationPercent),
    superGuaranteeRate: fromPercent(data.superGuaranteePercent),
    contributionsTaxRate: fromPercent(data.contributionsTaxPercent),
    lifeExpectancy: Number(data.lifeExpectancy) || 0,
    members: data.members.map(m => ({
        id: m.id,
        name: m.name,
        dateOfBirth: m.dateOfBirth,
        currentIncome: Number(m.currentIncome) || 0,
        retirementAge: Number(m.retirementAge) || 0,
        instrumentIds: m.instrumentIds ?? [],
    })),
});

export const RetirementSettingsModal: React.FC<RetirementSettingsModalProps> = ({ plan, currencyCode, show, onHide }) => {

    const { data: accounts } = useAccounts();
    const { updateAsync, isPending } = useUpdateRetirementPlan();

    const form = useForm<RetirementSettingsFormValues>({
        values: toFormValues(plan),
        resetOptions: { keepDirtyValues: true },
    });

    const { fields, append, remove } = useFieldArray({ control: form.control, name: "members" });

    const members = useWatch({ control: form.control, name: "members" });

    // Only superannuation accounts can back a retirement projection, so nothing else is offered.
    const superAccounts: LogicalAccount[] = (accounts ?? []).filter(a => a.accountType === "Superannuation");

    if (!plan) return null;

    const handleSave = async (data: RetirementSettingsFormValues) => {
        await updateAsync(plan.id, toRequest(data));
        onHide();
    };

    const handleHide = () => {
        form.reset(toFormValues(plan));
        onHide();
    };

    return (
        <Modal show={show} onHide={handleHide} size="lg" title="Edit Retirement Plan">
            <Form form={form} onSubmit={handleSave}>
                <Modal.Header closeButton>
                    <Modal.Title>Edit Retirement Plan</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    <Form.Group groupId="name">
                        <Form.Label>Plan Name</Form.Label>
                        <Form.Input type="text" />
                    </Form.Group>

                    <fieldset className="retirement-fieldset">
                        <legend>Assumptions</legend>
                        <div className="retirement-assumptions">
                            <Form.Group groupId="expectedReturnPercent">
                                <Form.Label>Expected Return (% a year)</Form.Label>
                                <Form.Input type="number" step="0.1" />
                            </Form.Group>
                            <Form.Group groupId="inflationPercent">
                                <Form.Label>Inflation (% a year)</Form.Label>
                                <Form.Input type="number" step="0.1" />
                            </Form.Group>
                            <Form.Group groupId="superGuaranteePercent">
                                <Form.Label>Employer Contribution (%)</Form.Label>
                                <Form.Input type="number" step="0.1" />
                            </Form.Group>
                            <Form.Group groupId="contributionsTaxPercent">
                                <Form.Label>Contributions Tax (%)</Form.Label>
                                <Form.Input type="number" step="0.1" />
                            </Form.Group>
                            <Form.Group groupId="lifeExpectancy">
                                <Form.Label>Savings Must Last Until Age</Form.Label>
                                <Form.Input type="number" step="1" />
                            </Form.Group>
                        </div>
                    </fieldset>

                    <fieldset className="retirement-fieldset">
                        <legend>People</legend>
                        {fields.length === 0 && (
                            <p className="retirement-empty">No one added yet. Add yourself, and your spouse if their superannuation should count towards the same retirement.</p>
                        )}
                        {fields.map((field, index) => (
                            <div className="retirement-member" key={field.id}>
                                <div className="retirement-member-fields">
                                    <Form.Group groupId={`members.${index}.name`}>
                                        <Form.Label>Name</Form.Label>
                                        <Form.Input type="text" />
                                    </Form.Group>
                                    <Form.Group groupId={`members.${index}.dateOfBirth`}>
                                        <Form.Label>Date of Birth</Form.Label>
                                        <Form.Input type="date" />
                                    </Form.Group>
                                    <Form.Group groupId={`members.${index}.currentIncome`}>
                                        <Form.Label>Current Income</Form.Label>
                                        <CurrencyInput currency={currencyCode} min={0} />
                                    </Form.Group>
                                    <Form.Group groupId={`members.${index}.retirementAge`}>
                                        <Form.Label>Retirement Age</Form.Label>
                                        <Form.Input type="number" step="1" />
                                    </Form.Group>
                                </div>
                                <Form.Group groupId={`members.${index}.instrumentIds`}>
                                    <Form.Label>Superannuation Accounts</Form.Label>
                                    <ComboBox
                                        multiSelect
                                        clearable
                                        placeholder="Select superannuation accounts..."
                                        items={superAccounts}
                                        selectedItems={superAccounts.filter(a => (members?.[index]?.instrumentIds ?? []).includes(a.id))}
                                        labelField={a => a?.name}
                                        valueField={a => a?.id}
                                        onChange={(items) => form.setValue(`members.${index}.instrumentIds`, items.map(a => a.id), { shouldDirty: true })}
                                    />
                                </Form.Group>
                                <div className="retirement-member-actions">
                                    <Button variant="outline-danger" onClick={() => remove(index)}>Remove</Button>
                                </div>
                            </div>
                        ))}
                        <Button
                            variant="outline-primary"
                            onClick={() => append({ name: "", dateOfBirth: "", currentIncome: 0, retirementAge: defaultRetirementAge, instrumentIds: [] })}
                        >
                            Add Person
                        </Button>
                    </fieldset>
                </Modal.Body>
                <Modal.Footer>
                    <Button variant="outline-primary" onClick={handleHide}>Close</Button>
                    <Button type="submit" variant="primary" disabled={isPending}>Save</Button>
                </Modal.Footer>
            </Form>
        </Modal>
    );
};
