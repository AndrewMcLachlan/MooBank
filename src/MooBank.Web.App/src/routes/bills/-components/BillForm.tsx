import React from "react";
import { Button, DeleteIcon, Form, Icon, Section, SectionForm } from "@andrewmclachlan/moo-ds";
import type { Control, UseFormReturn } from "react-hook-form";
import { useFieldArray } from "react-hook-form";

import type { ChargeType } from "api/types.gen";
import type { CreateBill, CreatePeriod, CreateServiceCharge, CreateUsage } from "models/bills";
import { UsageTypes } from "models/bills";
import { amountStep } from "utils/currency";

const defaultServiceCharge: CreateServiceCharge = { chargeTypeId: 1, chargePerDay: 0 };

const defaultUsage: CreateUsage = { usageType: "Consumption", pricePerUnit: 0, totalUsage: 0 };

export const emptyPeriod = (): CreatePeriod => ({
    periodStart: "",
    periodEnd: "",
    usages: [{ ...defaultUsage }],
    serviceCharges: [{ ...defaultServiceCharge }],
});

/** A labelled group of repeated rows, with the control that adds another. */
const RowGroup: React.FC<React.PropsWithChildren<{ label: string; addTitle: string; onAdd: () => void }>> = ({ label, addTitle, onAdd, children }) => (
    <div className="row-group">
        <div className="row-group-header">
            <span className="row-group-label">{label}</span>
            <Icon icon="plus" title={addTitle} onClick={onAdd} />
        </div>
        {children}
    </div>
);

interface UsagesProps {
    control: Control<CreateBill>;
    periodIndex: number;
}

const Usages: React.FC<UsagesProps> = ({ control, periodIndex }) => {

    const { fields, append, remove } = useFieldArray({ control, name: `periods.${periodIndex}.usages` });

    return (
        <RowGroup label="Usage" addTitle="Add export" onAdd={() => append({ ...defaultUsage, usageType: "Export" })}>
            {fields.map((field, index) => (
                <div key={field.id} className="entry-row usage-row">
                    <Form.Group groupId={`periods.${periodIndex}.usages.${index}.usageType`}>
                        <Form.Select>
                            {UsageTypes.map(t => <option key={t} value={t}>{t}</option>)}
                        </Form.Select>
                    </Form.Group>
                    <Form.Group groupId={`periods.${periodIndex}.usages.${index}.pricePerUnit`}>
                        <Form.Input type="number" step="0.00001" required placeholder="Price/unit" />
                    </Form.Group>
                    <Form.Group groupId={`periods.${periodIndex}.usages.${index}.totalUsage`}>
                        <Form.Input type="number" step="0.001" required placeholder="Units" />
                    </Form.Group>
                    <span className="entry-action">
                        {fields.length > 1 && <DeleteIcon onClick={() => remove(index)} />}
                    </span>
                </div>
            ))}
        </RowGroup>
    );
};

interface ServiceChargesProps {
    control: Control<CreateBill>;
    periodIndex: number;
    chargeTypes: ChargeType[];
}

const ServiceCharges: React.FC<ServiceChargesProps> = ({ control, periodIndex, chargeTypes }) => {

    const { fields, append, remove } = useFieldArray({ control, name: `periods.${periodIndex}.serviceCharges` });

    return (
        <RowGroup label="Service charges" addTitle="Add service charge" onAdd={() => append({ ...defaultServiceCharge })}>
            {fields.map((field, index) => (
                <div key={field.id} className="entry-row charge-row">
                    <Form.Group groupId={`periods.${periodIndex}.serviceCharges.${index}.chargeTypeId`}>
                        <Form.Select>
                            {chargeTypes.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
                        </Form.Select>
                    </Form.Group>
                    <Form.Group groupId={`periods.${periodIndex}.serviceCharges.${index}.chargePerDay`}>
                        <Form.Input type="number" step="0.00001" required placeholder="Charge/day" />
                    </Form.Group>
                    <span className="entry-action">
                        {fields.length > 1 && <DeleteIcon onClick={() => remove(index)} />}
                    </span>
                </div>
            ))}
        </RowGroup>
    );
};

export interface BillFormProps {
    form: UseFormReturn<CreateBill>;
    chargeTypes: ChargeType[];
    submitLabel: string;
    pending: boolean;
    onSubmit: (bill: CreateBill) => void | Promise<void>;
    onCancel: () => void;
    /** Rendered above the bill fields, for the account picker when adding. */
    header?: React.ReactNode;
}

/**
 * The fields of a bill, shared by adding and editing.
 *
 * Rows are added and removed with icons rather than buttons, which is how the rest of the app
 * handles a repeating row -- see the transaction split editor.
 *
 * A bill's cost and total usage are not among the fields: the database derives the cost from the
 * periods and the total from the readings, so both are ignored on the way in. Offering them as
 * inputs would mean typing a figure and watching it be discarded.
 */
export const BillForm: React.FC<BillFormProps> = ({ form, chargeTypes, submitLabel, pending, onSubmit, onCancel, header }) => {

    const { fields: periodFields, append: appendPeriod, remove: removePeriod } = useFieldArray({
        control: form.control,
        name: "periods",
    });

    const { fields: discountFields, append: appendDiscount, remove: removeDiscount } = useFieldArray({
        control: form.control,
        name: "discounts",
    });

    return (
        <SectionForm form={form} onSubmit={onSubmit} className="bill-form">
            {header}
            <div className="form-row">
                <Form.Group groupId="invoiceNumber">
                    <Form.Label>Invoice Number</Form.Label>
                    <Form.Input type="text" maxLength={11} />
                </Form.Group>
                <Form.Group groupId="issueDate">
                    <Form.Label>Issue Date</Form.Label>
                    <Form.Input type="date" required />
                </Form.Group>
            </div>
            <div className="form-row-3">
                <Form.Group groupId="previousReading">
                    <Form.Label>Previous Reading</Form.Label>
                    <Form.Input type="number" />
                </Form.Group>
                <Form.Group groupId="currentReading">
                    <Form.Label>Current Reading</Form.Label>
                    <Form.Input type="number" />
                </Form.Group>
                <Form.Group groupId="costsIncludeGST" className="form-check">
                    <Form.Check />
                    <Form.Label className="form-check-label">Costs Include GST</Form.Label>
                </Form.Group>
            </div>

            <Section header={
                <span className="section-header">
                    <span>Billing Periods</span>
                    <Icon icon="plus" title="Add period" onClick={() => appendPeriod(emptyPeriod())} />
                </span>
            }>
                {periodFields.map((field, index) => (
                    <div key={field.id} className="period-entry">
                        <div className="entry-row period-row">
                            <Form.Group groupId={`periods.${index}.periodStart`}>
                                <Form.Label>Period Start</Form.Label>
                                <Form.Input type="date" required />
                            </Form.Group>
                            <Form.Group groupId={`periods.${index}.periodEnd`}>
                                <Form.Label>Period End</Form.Label>
                                <Form.Input type="date" required />
                            </Form.Group>
                            <span className="entry-action">
                                {periodFields.length > 1 && <DeleteIcon onClick={() => removePeriod(index)} />}
                            </span>
                        </div>
                        <Usages control={form.control} periodIndex={index} />
                        <ServiceCharges control={form.control} periodIndex={index} chargeTypes={chargeTypes} />
                    </div>
                ))}
            </Section>

            <Section header={
                <span className="section-header">
                    <span>Discounts</span>
                    <Icon icon="plus" title="Add discount" onClick={() => appendDiscount({ discountPercent: undefined, discountAmount: undefined, reason: "" })} />
                </span>
            }>
                {discountFields.length === 0 && (
                    <p className="empty-message">No discounts added.</p>
                )}
                {discountFields.map((field, index) => (
                    <div key={field.id} className="entry-row discount-row">
                        <Form.Group groupId={`discounts.${index}.discountPercent`}>
                            <Form.Input type="number" min={0} max={100} placeholder="Discount %" />
                        </Form.Group>
                        <Form.Group groupId={`discounts.${index}.discountAmount`}>
                            <Form.Input type="number" step={amountStep} placeholder="Amount" />
                        </Form.Group>
                        <Form.Group groupId={`discounts.${index}.reason`}>
                            <Form.Input type="text" maxLength={255} placeholder="Reason" />
                        </Form.Group>
                        <span className="entry-action">
                            <DeleteIcon onClick={() => removeDiscount(index)} />
                        </span>
                    </div>
                ))}
            </Section>

            <div className="form-actions">
                <Button type="submit" variant="primary" disabled={pending}>{submitLabel}</Button>
                <Button type="button" variant="secondary" onClick={onCancel}>Cancel</Button>
            </div>
        </SectionForm>
    );
};
