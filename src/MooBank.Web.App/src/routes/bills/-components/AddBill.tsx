import React, { useState, useEffect } from "react";
import { Button, Modal } from "@andrewmclachlan/moo-ds";
import type { Control } from "react-hook-form";
import { useFieldArray, useForm } from "react-hook-form";
import { format } from "date-fns";

import { Form, Section, SectionForm } from "@andrewmclachlan/moo-ds";

import type { ChargeType } from "api/types.gen";
import type { CreateBill, CreatePeriod, CreateServiceCharge, CreateUsage } from "models/bills";
import { UsageTypes } from "models/bills";
import { useCreateBill } from "../-hooks/useCreateBill";
import { useBillAccounts } from "../-hooks/useBillAccounts";
import { useChargeTypes } from "../-hooks/useChargeTypes";
import { amountStep } from "utils/currency";

export interface AddBillProps {
    accountId?: string;
    show: boolean;
    onHide: () => void;
}

const defaultServiceCharge: CreateServiceCharge = { chargeTypeId: 1, chargePerDay: 0 };

const defaultUsage: CreateUsage = { usageType: "Consumption", pricePerUnit: 0, totalUsage: 0 };

const emptyPeriod = (): CreatePeriod => ({
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

export const AddBill: React.FC<AddBillProps> = ({ accountId, show, onHide }) => {

    const createBill = useCreateBill();
    const { data: accounts } = useBillAccounts();
    const { data: chargeTypes } = useChargeTypes();
    const [selectedAccountId, setSelectedAccountId] = useState<string>(accountId ?? "");

    useEffect(() => {
        if (accountId) {
            setSelectedAccountId(accountId);
        } else if (accounts && accounts.length > 0 && !selectedAccountId) {
            setSelectedAccountId(accounts[0].id);
        }
    }, [accountId, accounts, selectedAccountId]);

    const form = useForm<CreateBill>({
        defaultValues: {
            issueDate: format(new Date(), "yyyy-MM-dd"),
            total: 0,
            cost: 0,
            costsIncludeGST: true,
            periods: [emptyPeriod()],
            discounts: [],
        }
    });

    const { fields: periodFields, append: appendPeriod, remove: removePeriod } = useFieldArray({
        control: form.control,
        name: "periods",
    });

    const { fields: discountFields, append: appendDiscount, remove: removeDiscount } = useFieldArray({
        control: form.control,
        name: "discounts",
    });

    const handleSubmit = async (data: CreateBill) => {
        if (!selectedAccountId) return;
        await createBill.mutateAsync(selectedAccountId, data);
        form.reset();
        onHide();
    };

    const handleClose = () => {
        form.reset();
        onHide();
    };

    const addPeriod = () => {
        appendPeriod(emptyPeriod());
    };

    const addDiscount = () => {
        appendDiscount({ discountPercent: undefined, discountAmount: undefined, reason: "" });
    };

    return (
        <Modal show={show} onHide={handleClose} size="lg">
            <Modal.Header closeButton>
                <Modal.Title>Add Bill</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <SectionForm form={form} onSubmit={handleSubmit} className="bill-form">
                    {!accountId && (
                        <div className="form-row">
                            <Form.Group groupId="account">
                                <Form.Label>Account</Form.Label>
                                <Form.Select value={selectedAccountId} onChange={(e) => setSelectedAccountId(e.target.value)} required>
                                    {accounts?.map(account => (
                                        <option key={account.id} value={account.id}>{account.name}</option>
                                    ))}
                                </Form.Select>
                            </Form.Group>
                        </div>
                    )}
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
                    <div className="form-row">
                        <Form.Group groupId="previousReading">
                            <Form.Label>Previous Reading</Form.Label>
                            <Form.Input type="number" />
                        </Form.Group>
                        <Form.Group groupId="currentReading">
                            <Form.Label>Current Reading</Form.Label>
                            <Form.Input type="number" />
                        </Form.Group>
                    </div>
                    <div className="form-row-3">
                        <Form.Group groupId="total">
                            <Form.Label>Total (calculated)</Form.Label>
                            <Form.Input type="number" />
                        </Form.Group>
                        <Form.Group groupId="cost">
                            <Form.Label>Cost</Form.Label>
                            <Form.Input type="number" step="0.01" required />
                        </Form.Group>
                        <Form.Group groupId="costsIncludeGST" className="form-check">
                            <Form.Check />
                            <Form.Label className="form-check-label">Costs Include GST</Form.Label>
                        </Form.Group>
                    </div>

                    <Section header={
                        <span className="section-header">
                            <span>Billing Periods</span>
                            <Button variant="outline-primary" size="sm" onClick={addPeriod} type="button">Add Period</Button>
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
                                <ServiceCharges control={form.control} periodIndex={index} chargeTypes={chargeTypes ?? []} />
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
                            <Button variant="outline-primary" size="sm" onClick={addDiscount} type="button">Add Discount</Button>
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
                        <Button type="submit" variant="primary" disabled={createBill.isPending}>Create Bill</Button>
                        <Button type="button" variant="secondary" onClick={handleClose}>Cancel</Button>
                    </div>
                </SectionForm>
            </Modal.Body>
        </Modal>
    );
};
