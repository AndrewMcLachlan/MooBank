import React from "react";
import { Button, Form, Section, SectionForm } from "@andrewmclachlan/moo-ds";
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

interface UsagesProps {
    control: Control<CreateBill>;
    periodIndex: number;
}

const Usages: React.FC<UsagesProps> = ({ control, periodIndex }) => {

    const { fields, append, remove } = useFieldArray({ control, name: `periods.${periodIndex}.usages` });

    return (
        <div className="bill-usages">
            {fields.map((field, index) => (
                <div key={field.id} className="form-row">
                    <Form.Group groupId={`periods.${periodIndex}.usages.${index}.usageType`}>
                        <Form.Label>Usage</Form.Label>
                        <Form.Select>
                            {UsageTypes.map(t => <option key={t} value={t}>{t}</option>)}
                        </Form.Select>
                    </Form.Group>
                    <Form.Group groupId={`periods.${periodIndex}.usages.${index}.pricePerUnit`}>
                        <Form.Label>Price/Unit</Form.Label>
                        <Form.Input type="number" step="0.00001" required />
                    </Form.Group>
                    <Form.Group groupId={`periods.${periodIndex}.usages.${index}.totalUsage`}>
                        <Form.Label>Total Units</Form.Label>
                        <Form.Input type="number" step="0.001" required />
                    </Form.Group>
                    {fields.length > 1 && (
                        <Button variant="outline-danger" size="sm" onClick={() => remove(index)} type="button" className="remove-button">
                            Remove
                        </Button>
                    )}
                </div>
            ))}
            <Button variant="outline-primary" size="sm" onClick={() => append({ ...defaultUsage, usageType: "Export" })} type="button">Add Export</Button>
        </div>
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
        <div className="service-charges">
            {fields.map((field, index) => (
                <div key={field.id} className="form-row">
                    <Form.Group groupId={`periods.${periodIndex}.serviceCharges.${index}.chargeTypeId`}>
                        <Form.Label>Service Charge</Form.Label>
                        <Form.Select>
                            {chargeTypes.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
                        </Form.Select>
                    </Form.Group>
                    <Form.Group groupId={`periods.${periodIndex}.serviceCharges.${index}.chargePerDay`}>
                        <Form.Label>Charge/Day</Form.Label>
                        <Form.Input type="number" step="0.00001" required />
                    </Form.Group>
                    {fields.length > 1 && (
                        <Button variant="outline-danger" size="sm" onClick={() => remove(index)} type="button" className="remove-button">
                            Remove
                        </Button>
                    )}
                </div>
            ))}
            <Button variant="outline-primary" size="sm" onClick={() => append({ ...defaultServiceCharge })} type="button">Add Service Charge</Button>
        </div>
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
 * A bill's cost and total usage are not among them: the database derives the cost from the periods
 * and the total from the readings, so both are ignored on the way in. Offering them as inputs would
 * mean typing a figure and watching it be discarded.
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
                    <Button variant="outline-primary" size="sm" onClick={() => appendPeriod(emptyPeriod())} type="button">Add Period</Button>
                </span>
            }>
                {periodFields.map((field, index) => (
                    <div key={field.id} className="period-entry">
                        <div className="form-row">
                            <Form.Group groupId={`periods.${index}.periodStart`}>
                                <Form.Label>Period Start</Form.Label>
                                <Form.Input type="date" required />
                            </Form.Group>
                            <Form.Group groupId={`periods.${index}.periodEnd`}>
                                <Form.Label>Period End</Form.Label>
                                <Form.Input type="date" required />
                            </Form.Group>
                        </div>
                        <Usages control={form.control} periodIndex={index} />
                        <ServiceCharges control={form.control} periodIndex={index} chargeTypes={chargeTypes} />
                        {periodFields.length > 1 && (
                            <Button variant="outline-danger" size="sm" onClick={() => removePeriod(index)} type="button" className="remove-button">
                                Remove Period
                            </Button>
                        )}
                    </div>
                ))}
            </Section>

            <Section header={
                <span className="section-header">
                    <span>Discounts</span>
                    <Button variant="outline-primary" size="sm" onClick={() => appendDiscount({ discountPercent: undefined, discountAmount: undefined, reason: "" })} type="button">Add Discount</Button>
                </span>
            }>
                {discountFields.length === 0 && (
                    <p className="empty-message">No discounts added.</p>
                )}
                {discountFields.map((field, index) => (
                    <div key={field.id} className="discount-entry">
                        <div className="form-row-3">
                            <Form.Group groupId={`discounts.${index}.discountPercent`}>
                                <Form.Label>Discount %</Form.Label>
                                <Form.Input type="number" min={0} max={100} />
                            </Form.Group>
                            <Form.Group groupId={`discounts.${index}.discountAmount`}>
                                <Form.Label>Discount Amount</Form.Label>
                                <Form.Input type="number" step={amountStep} />
                            </Form.Group>
                            <Form.Group groupId={`discounts.${index}.reason`}>
                                <Form.Label>Reason</Form.Label>
                                <Form.Input type="text" maxLength={255} />
                            </Form.Group>
                        </div>
                        <Button variant="outline-danger" size="sm" onClick={() => removeDiscount(index)} type="button" className="remove-button">
                            Remove Discount
                        </Button>
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
