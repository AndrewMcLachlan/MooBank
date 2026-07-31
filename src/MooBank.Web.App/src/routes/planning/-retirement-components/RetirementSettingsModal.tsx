import { Button, ComboBox, Form, Modal } from "@andrewmclachlan/moo-ds";
import { useFieldArray, useForm, useWatch } from "react-hook-form";
import type { GrowthStrategy, LogicalAccount, RetirementPlan, RetirementProjectionSummary, SimpleRetirementPlan, User } from "api/types.gen";
import { useUpdateRetirementPlan } from "../-retirement-hooks/useUpdateRetirementPlan";
import { useAccounts } from "hooks/useAccounts";
import { useFamilyMembers } from "../-retirement-hooks/useFamilyMembers";
import { CurrencyInput } from "components";
import { defaultCurrentAge, defaultRetirementAge, fromPercent, growthStrategies, toPercent } from "../-retirement-utils/retirementDefaults";
import { ageForIncome, incomeForAge, type SyncBasis } from "../-retirement-utils/retirementSync";

/** A person's name for the picker, falling back to their email when they have not set one. */
const displayName = (u: User) => [u.firstName, u.lastName].filter(Boolean).join(" ") || u.emailAddress;

interface RetirementSettingsModalProps {
    plan?: RetirementPlan;
    /** The projection behind the plan, which is where the target-income solve gets its figures. */
    summary?: RetirementProjectionSummary;
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
    targetRetirementIncome: number;
    preRetirementSwitchYears: number;
    cashReturnPercent: number;
    members: {
        id?: string;
        userId: string;
        currentAge: number;
        salarySacrifice: number;
        growthStrategy: GrowthStrategy;
        annualFees: number;
        insurancePremium: number;
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
    targetRetirementIncome: plan?.targetRetirementIncome ?? 0,
    preRetirementSwitchYears: plan?.preRetirementSwitchYears ?? 2,
    cashReturnPercent: toPercent(plan?.cashReturnRate),
    members: (plan?.members ?? []).map(m => ({
        id: m.id,
        userId: m.userId,
        currentAge: m.currentAge,
        salarySacrifice: m.salarySacrifice,
        growthStrategy: m.growthStrategy,
        annualFees: m.annualFees,
        insurancePremium: m.insurancePremium,
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
    targetRetirementIncome: Number(data.targetRetirementIncome) || 0,
    preRetirementSwitchYears: Number(data.preRetirementSwitchYears) || 0,
    cashReturnRate: fromPercent(data.cashReturnPercent),
    members: data.members.map(m => ({
        id: m.id,
        // Null rather than the empty string the select carries for "not chosen yet": an empty string
        // is not a readable id, so the request would fail to parse before validation could say so.
        userId: m.userId || null,
        currentAge: Number(m.currentAge) || 0,
        salarySacrifice: Number(m.salarySacrifice) || 0,
        growthStrategy: m.growthStrategy,
        annualFees: Number(m.annualFees) || 0,
        insurancePremium: Number(m.insurancePremium) || 0,
        currentIncome: Number(m.currentIncome) || 0,
        retirementAge: Number(m.retirementAge) || 0,
        instrumentIds: m.instrumentIds ?? [],
    })),
});

export const RetirementSettingsModal: React.FC<RetirementSettingsModalProps> = ({ plan, summary, currencyCode, show, onHide }) => {

    const { data: accounts } = useAccounts();
    const { members: familyMembers } = useFamilyMembers();
    const { updateAsync, isPending } = useUpdateRetirementPlan();

    const form = useForm<RetirementSettingsFormValues>({
        values: toFormValues(plan),
        resetOptions: { keepDirtyValues: true },
    });

    const { fields, append, remove } = useFieldArray({ control: form.control, name: "members" });

    const members = useWatch({ control: form.control, name: "members" });

    /**
     * The target income and the age the savings must last to are two ends of one equation, so editing
     * either solves the other — the same link the sliders on the plan page use, so the two screens
     * cannot disagree about it.
     */
    const basis: SyncBasis | undefined = summary && summary.balanceAtRetirementInTodaysDollars > 0
        ? { balance: summary.balanceAtRetirementInTodaysDollars, realReturnRate: summary.drawdownRealReturnRate, retirementAge: summary.retirementAge }
        : undefined;

    const syncFromLifeExpectancy = (age: number) => {
        if (!basis || !age) return;

        form.setValue("targetRetirementIncome", incomeForAge(basis, age), { shouldDirty: true });
    };

    const syncFromTargetIncome = (income: number) => {
        if (!basis || !income) return;

        const age = ageForIncome(basis, income);

        // Nothing to set when the balance never runs down, or lasts past any age a plan can hold.
        if (age !== null) {
            form.setValue("lifeExpectancy", age, { shouldDirty: true });
        }
    };

    // Only superannuation accounts can back a retirement projection, so nothing else is offered.
    const superAccounts: LogicalAccount[] = (accounts ?? []).filter(a => a.accountType === "Superannuation");

    /**
     * The accounts a member can be credited with: the superannuation accounts the selected person
     * owns. The server enforces the same rule, so this keeps the form from offering a choice that
     * would be rejected — and stops one person being credited with another's balance.
     */
    const accountsFor = (userId: string) => {
        const owner = familyMembers.find(u => u.id === userId);
        if (!owner) return [];

        return superAccounts.filter(a => owner.accounts.includes(a.id));
    };

    if (!plan) return null;

    /**
     * A person nobody has been chosen for cannot be saved.
     *
     * Held back here rather than sent and refused: an unchosen person carries no readable id, so the
     * request would be rejected before any rule could explain why, and the caller would see a bare
     * failure instead of being told which row is unfinished.
     */
    const unchosen = (members ?? []).some(m => !m.userId);

    const handleSave = async (data: RetirementSettingsFormValues) => {
        if (data.members.some(m => !m.userId)) return;

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
                                <Form.Input type="number" step="1" onBlur={e => syncFromLifeExpectancy(Number(e.target.value))} />
                            </Form.Group>
                        </div>
                    </fieldset>

                    <fieldset className="retirement-fieldset">
                        <legend>Retirement</legend>
                        <div className="retirement-assumptions">
                            <Form.Group groupId="targetRetirementIncome">
                                <Form.Label>Target Income (a year, today's dollars)</Form.Label>
                                <CurrencyInput currency={currencyCode} onBlur={e => syncFromTargetIncome(Number(e.target.value))} />
                            </Form.Group>
                            <Form.Group groupId="preRetirementSwitchYears">
                                <Form.Label>Years Switched to Cash Before Retiring</Form.Label>
                                <Form.Input type="number" step="1" min="0" />
                            </Form.Group>
                            <Form.Group groupId="cashReturnPercent">
                                <Form.Label>Cash Return (% a year)</Form.Label>
                                <Form.Input type="number" step="0.1" />
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
                                    <Form.Group groupId={`members.${index}.userId`}>
                                        <Form.Label>Person</Form.Label>
                                        <Form.Select>
                                            <option value="">Select a person…</option>
                                            {familyMembers.map(u => (
                                                <option key={u.id} value={u.id}>{displayName(u)}</option>
                                            ))}
                                        </Form.Select>
                                    </Form.Group>
                                    <Form.Group groupId={`members.${index}.currentAge`}>
                                        <Form.Label>Current Age</Form.Label>
                                        <Form.Input type="number" step="1" />
                                    </Form.Group>
                                    <Form.Group groupId={`members.${index}.currentIncome`}>
                                        <Form.Label>Current Income</Form.Label>
                                        <CurrencyInput currency={currencyCode} min={0} />
                                    </Form.Group>
                                    <Form.Group groupId={`members.${index}.salarySacrifice`}>
                                        <Form.Label>Salary Sacrifice (a year)</Form.Label>
                                        <CurrencyInput currency={currencyCode} min={0} />
                                    </Form.Group>
                                    <Form.Group groupId={`members.${index}.retirementAge`}>
                                        <Form.Label>Retirement Age</Form.Label>
                                        <Form.Input type="number" step="1" />
                                    </Form.Group>
                                    <Form.Group groupId={`members.${index}.growthStrategy`}>
                                        <Form.Label>Growth Strategy</Form.Label>
                                        <Form.Select>
                                            {growthStrategies.map(s => <option key={s.value} value={s.value}>{s.label}</option>)}
                                        </Form.Select>
                                    </Form.Group>
                                    <Form.Group groupId={`members.${index}.annualFees`}>
                                        <Form.Label>Fund Fees (a year)</Form.Label>
                                        <CurrencyInput currency={currencyCode} min={0} />
                                    </Form.Group>
                                    <Form.Group groupId={`members.${index}.insurancePremium`}>
                                        <Form.Label>Insurance Premium (a year)</Form.Label>
                                        <CurrencyInput currency={currencyCode} min={0} />
                                    </Form.Group>
                                </div>
                                <Form.Group groupId={`members.${index}.instrumentIds`}>
                                    <Form.Label>Superannuation Accounts</Form.Label>
                                    <ComboBox
                                        multiSelect
                                        clearable
                                        placeholder="Select superannuation accounts..."
                                        items={accountsFor(members?.[index]?.userId ?? "")}
                                        selectedItems={accountsFor(members?.[index]?.userId ?? "").filter(a => (members?.[index]?.instrumentIds ?? []).includes(a.id))}
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
                        {unchosen && (
                            <p className="retirement-empty">Choose a person for everyone on the plan before saving.</p>
                        )}
                        <Button
                            variant="outline-primary"
                            onClick={() => append({ userId: "", currentAge: defaultCurrentAge, currentIncome: 0, salarySacrifice: 0, retirementAge: defaultRetirementAge, growthStrategy: "Balanced", annualFees: 0, insurancePremium: 0, instrumentIds: [] })}
                        >
                            Add Person
                        </Button>
                    </fieldset>
                </Modal.Body>
                <Modal.Footer>
                    <Button variant="outline-primary" onClick={handleHide}>Close</Button>
                    <Button type="submit" variant="primary" disabled={isPending || unchosen}>Save</Button>
                </Modal.Footer>
            </Form>
        </Modal>
    );
};
