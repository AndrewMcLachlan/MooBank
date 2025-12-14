import { SectionTable, DeleteIcon, EditColumn, useUpdatingState } from "@andrewmclachlan/moo-ds";
import { format, parseISO } from "date-fns";
import { PlannedItem, ScheduleFrequency } from "models";
import { useState } from "react";
import { Form } from "react-bootstrap";
import { useDeletePlannedItem, useUpdatePlannedItem } from "services/ForecastService";
import { NewPlannedItem } from "./NewPlannedItem";

interface PlannedItemsTableProps {
    planId: string;
    items: PlannedItem[];
}

export const PlannedItemsTable: React.FC<PlannedItemsTableProps> = ({ planId, items }) => {
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
                    <th className="column-25">Name</th>
                    <th className="column-15">Amount</th>
                    <th className="column-15">When</th>
                    <th className="column-15">Frequency</th>
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
                    <td colSpan={4}></td>
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
        setItem(updated);
        update(planId, item.id, updated);
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
            handleUpdate({ fixedDate: value });
        } else if (item.dateMode === "Schedule") {
            handleUpdate({ scheduleAnchorDate: value });
        }
    };

    const formatDateDisplay = (): string => {
        switch (item.dateMode) {
            case "FixedDate":
                return item.fixedDate ? format(parseISO(item.fixedDate), "dd MMM yyyy") : "-";
            case "Schedule":
                return item.scheduleAnchorDate ? format(parseISO(item.scheduleAnchorDate), "dd MMM yyyy") : "-";
            case "FlexibleWindow":
                if (item.windowStartDate && item.windowEndDate) {
                    return `${format(parseISO(item.windowStartDate), "MMM yy")} - ${format(parseISO(item.windowEndDate), "MMM yy")}`;
                }
                return "-";
            default:
                return "-";
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
            {item.dateMode !== "FlexibleWindow" ? (
                <EditColumn
                    type="date"
                    value={getDateValue()}
                    onChange={(v) => handleDateChange(v.value)}
                >
                    {formatDateDisplay()}
                </EditColumn>
            ) : (
                <td>{formatDateDisplay()}</td>
            )}
            <td onClick={() => !isEditingFrequency && setIsEditingFrequency(true)}>
                {isEditingFrequency ? (
                    <Form.Select
                        size="sm"
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
                    </Form.Select>
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
