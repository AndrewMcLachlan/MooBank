import { SectionTable, DeleteIcon, EditColumn, useUpdatingState } from "@andrewmclachlan/moo-ds";
import { format, parseISO } from "date-fns";
import type { ForecastPlan, PlannedItem, ScheduleFrequency } from "api/types.gen";
import { useState } from "react";
import { Input } from "@andrewmclachlan/moo-ds";
import { useUpdatePlannedItem } from "../-hooks/useUpdatePlannedItem";
import { useDeletePlannedItem } from "../-hooks/useDeletePlannedItem";
import { NewPlannedItem } from "./NewPlannedItem";

interface PlannedItemsTableProps {
    plan?: ForecastPlan;
}

export const PlannedItemsTable: React.FC<PlannedItemsTableProps> = ({ plan }) => {

    const items = plan?.plannedItems ?? [];
    const planId = plan?.id;

    const incomeItems = items.filter(i => i.itemType === "Income");
    const expenseItems = items.filter(i => i.itemType === "Expense");

    return (
        <>
            <PlannedItemsSection planId={planId} title="Planned Income" items={incomeItems} itemType="Income" />
            <PlannedItemsSection planId={planId} title="Planned Expenses" items={expenseItems} itemType="Expense" />
        </>
    );
};

interface PlannedItemsSectionProps {
    planId: string;
    title: string;
    items: PlannedItem[];
    itemType: "Income" | "Expense";
}

const PlannedItemsSection: React.FC<PlannedItemsSectionProps> = ({ planId, title, items, itemType }) => {
    return (
        <SectionTable header={title} striped>
            <thead>
                <tr>
                    <th className="column-20">Name</th>
                    <th className="column-10">Amount</th>
                    <th className="column-12">Start Date</th>
                    <th className="column-12">End Date</th>
                    <th className="column-12">Frequency</th>
                    <th className="column-20">Notes</th>
                    <th className="row-action"></th>
                </tr>
            </thead>
            <tbody>
                {items.map((item) => (
                    <PlannedItemRow key={item.id} planId={planId} item={item} />
                ))}
                <NewPlannedItem planId={planId} itemType={itemType} />
            </tbody>
            <tfoot>
                <tr>
                    <td>Total</td>
                    <td className="amount">${items.reduce((sum, i) => sum + (i.isIncluded ? i.amount : 0), 0).toLocaleString(undefined, { minimumFractionDigits: 2 })}</td>
                    <td colSpan={5}></td>
                </tr>
            </tfoot>
        </SectionTable>
    );
};

interface PlannedItemRowProps {
    planId: string;
    item: PlannedItem;
}

const PlannedItemRow: React.FC<PlannedItemRowProps> = ({ planId, item: propItem }) => {
    const [item, setItem] = useUpdatingState(propItem);
    const { update } = useUpdatePlannedItem();
    const deleteItem = useDeletePlannedItem();
    const [isEditingFrequency, setIsEditingFrequency] = useState(false);

    const handleDelete = () => {
        if (confirm(`Are you sure you want to delete "${item.name}"?`)) {
            deleteItem(planId, item.id);
        }
    };

    const handleUpdate = (changes: Partial<PlannedItem>) => {
        const updated = { ...item, ...changes };
        // Clean up empty string dates to undefined for proper JSON serialization
        const cleaned: Partial<PlannedItem> = {
            ...updated,
            fixedDate: updated.fixedDate || undefined,
            scheduleAnchorDate: updated.scheduleAnchorDate || undefined,
            scheduleEndDate: updated.scheduleEndDate || undefined,
            windowStartDate: updated.windowStartDate || undefined,
            windowEndDate: updated.windowEndDate || undefined,
        };
        setItem(cleaned as PlannedItem);
        update(planId, item.id, cleaned);
    };

    const getDateValue = (): string => {
        switch (item.dateMode) {
            case "FixedDate":
                return item.fixedDate ?? "";
            case "Schedule":
                return item.scheduleAnchorDate ?? "";
            default:
                return "";
        }
    };

    const handleDateChange = (value: string) => {
        if (item.dateMode === "FixedDate") {
            handleUpdate({ fixedDate: value || undefined });
        } else if (item.dateMode === "Schedule") {
            handleUpdate({ scheduleAnchorDate: value || undefined });
        }
    };

    const formatStartDateDisplay = (): string => {
        switch (item.dateMode) {
            case "FixedDate":
                return item.fixedDate ? format(parseISO(item.fixedDate), "dd MMM yyyy") : "-";
            case "Schedule":
                return item.scheduleAnchorDate ? format(parseISO(item.scheduleAnchorDate), "dd MMM yyyy") : "-";
            case "FlexibleWindow":
                return item.windowStartDate ? format(parseISO(item.windowStartDate), "dd MMM yyyy") : "-";
            default:
                return "-";
        }
    };

    const formatEndDateDisplay = (): string => {
        switch (item.dateMode) {
            case "FixedDate":
                return "-";
            case "Schedule":
                return item.scheduleEndDate ? format(parseISO(item.scheduleEndDate), "dd MMM yyyy") : "Ongoing";
            case "FlexibleWindow":
                return item.windowEndDate ? format(parseISO(item.windowEndDate), "dd MMM yyyy") : "-";
            default:
                return "-";
        }
    };

    const getEndDateValue = (): string => {
        switch (item.dateMode) {
            case "Schedule":
                return item.scheduleEndDate ?? "";
            case "FlexibleWindow":
                return item.windowEndDate ?? "";
            default:
                return "";
        }
    };

    const handleEndDateChange = (value: string) => {
        if (item.dateMode === "Schedule") {
            handleUpdate({ scheduleEndDate: value || undefined });
        } else if (item.dateMode === "FlexibleWindow") {
            handleUpdate({ windowEndDate: value || undefined });
        }
    };

    const getFrequencyValue = (): string => {
        if (item.dateMode === "FixedDate") {
            return "FixedDate";
        }
        return item.scheduleFrequency ?? "Monthly";
    };

    const getFrequencyDisplay = (): string => {
        if (item.dateMode === "FixedDate") {
            return "One-time";
        }
        return item.scheduleFrequency ?? "Monthly";
    };

    const handleFrequencyChange = (value: string) => {
        if (value === "FixedDate") {
            handleUpdate({
                dateMode: "FixedDate",
                fixedDate: item.scheduleAnchorDate ?? format(new Date(), "yyyy-MM-dd"),
                scheduleFrequency: undefined,
                scheduleAnchorDate: undefined,
                scheduleInterval: undefined
            });
        } else {
            handleUpdate({
                dateMode: "Schedule",
                scheduleFrequency: value as ScheduleFrequency,
                scheduleAnchorDate: item.fixedDate ?? item.scheduleAnchorDate ?? format(new Date(), "yyyy-MM-dd"),
                scheduleInterval: 1,
                fixedDate: undefined
            });
        }
        setIsEditingFrequency(false);
    };

    return (
        <tr className={!item.isIncluded ? "text-muted" : ""}>
            <EditColumn
                value={item.name}
                onChange={(v) => handleUpdate({ name: v.value })}
            />
            <EditColumn
                className="amount"
                type="number"
                value={item.amount.toFixed(2)}
                onChange={(v) => handleUpdate({ amount: parseFloat(v.value) || 0 })}
            />
            <EditColumn
                type="date"
                value={getDateValue()}
                onChange={(v) => handleDateChange(v.value)}
            >
                {formatStartDateDisplay()}
            </EditColumn>
            {item.dateMode === "FixedDate" ? (
                <td className="text-muted">-</td>
            ) : (
                <EditColumn
                    type="date"
                    value={getEndDateValue()}
                    onChange={(v) => handleEndDateChange(v.value)}
                >
                    {formatEndDateDisplay()}
                </EditColumn>
            )}
            <td onClick={() => !isEditingFrequency && setIsEditingFrequency(true)}>
                {isEditingFrequency ? (
                    <Input.Select
                        autoFocus
                        value={getFrequencyValue()}
                        onChange={(e) => handleFrequencyChange(e.target.value)}
                        onBlur={() => setIsEditingFrequency(false)}
                    >
                        <option value="FixedDate">One-time</option>
                        <option value="Weekly">Weekly</option>
                        <option value="Fortnightly">Fortnightly</option>
                        <option value="Monthly">Monthly</option>
                        <option value="Yearly">Yearly</option>
                    </Input.Select>
                ) : (
                    <span className="clickable">{getFrequencyDisplay()}</span>
                )}
            </td>
            <EditColumn
                value={item.notes ?? ""}
                onChange={(v) => handleUpdate({ notes: v.value })}
            />
            <td className="row-action">
                <DeleteIcon onClick={handleDelete} />
            </td>
        </tr>
    );
};
