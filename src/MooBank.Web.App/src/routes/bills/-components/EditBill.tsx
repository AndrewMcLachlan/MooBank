import React from "react";
import { Modal } from "@andrewmclachlan/moo-ds";
import { useForm } from "react-hook-form";
import { parseISO } from "date-fns";

import type { Bill } from "api/types.gen";
import type { CreateBill } from "models/bills";
import { formatISODate } from "utils/dateFns";
import { useChargeTypes } from "../-hooks/useChargeTypes";
import { useUpdateBill } from "../-hooks/useUpdateBill";
import { BillForm } from "./BillForm";

export interface EditBillProps {
    accountId: string;
    bill: Bill;
    show: boolean;
    onHide: () => void;
}

/**
 * The bill as the form wants it: date inputs take yyyy-MM-dd, while periods come back from the API
 * as full timestamps.
 */
const toFormValues = (bill: Bill): CreateBill => ({
    invoiceNumber: bill.invoiceNumber ?? undefined,
    issueDate: formatISODate(parseISO(bill.issueDate)),
    currentReading: bill.currentReading ?? undefined,
    previousReading: bill.previousReading ?? undefined,
    costsIncludeGST: bill.costsIncludeGST ?? undefined,
    periods: (bill.periods ?? []).map(p => ({
        periodStart: formatISODate(parseISO(p.periodStart)),
        periodEnd: formatISODate(parseISO(p.periodEnd)),
        usages: (p.usages ?? []).map(u => ({
            usageType: u.usageType,
            pricePerUnit: u.pricePerUnit,
            totalUsage: u.totalUsage,
        })),
        serviceCharges: (p.serviceCharges ?? []).map(sc => ({
            chargeTypeId: sc.chargeTypeId,
            chargePerDay: sc.chargePerDay,
        })),
    })),
    discounts: (bill.discounts ?? []).map(d => ({
        discountPercent: d.discountPercent ?? undefined,
        discountAmount: d.discountAmount ?? undefined,
        reason: d.reason ?? undefined,
    })),
} as CreateBill);

export const EditBill: React.FC<EditBillProps> = ({ accountId, bill, show, onHide }) => {

    const updateBill = useUpdateBill();
    const { data: chargeTypes } = useChargeTypes();

    // values, not defaultValues: the bill arrives from a query, and keepDirtyValues stops a
    // background refetch from overwriting what is being typed.
    const form = useForm<CreateBill>({
        values: toFormValues(bill),
        resetOptions: { keepDirtyValues: true },
    });

    const handleSubmit = async (data: CreateBill) => {
        await updateBill.mutateAsync(accountId, bill.id, data);
        onHide();
    };

    return (
        <Modal show={show} onHide={onHide} size="lg">
            <Modal.Header closeButton>
                <Modal.Title>{bill.invoiceNumber ? `Edit Bill #${bill.invoiceNumber}` : "Edit Bill"}</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <BillForm
                    form={form}
                    chargeTypes={chargeTypes ?? []}
                    submitLabel="Save Bill"
                    pending={updateBill.isPending}
                    onSubmit={handleSubmit}
                    onCancel={onHide}
                />
            </Modal.Body>
        </Modal>
    );
};
