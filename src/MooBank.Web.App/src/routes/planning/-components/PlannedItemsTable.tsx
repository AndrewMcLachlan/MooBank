import { SectionTable, DeleteIcon, EditColumn, ComboBox, useUpdatingState } from "@andrewmclachlan/moo-ds";
import { format, parseISO } from "date-fns";
import type { ForecastPlan, PlannedItem, PlannedItemProgress, ScheduleFrequency, Tag } from "api/types.gen";
import { useState } from "react";
import { Input } from "@andrewmclachlan/moo-ds";
import { useTags } from "hooks/useTags";
import { useUpdatePlannedItem } from "../-hooks/useUpdatePlannedItem";
import { useDeletePlannedItem } from "../-hooks/useDeletePlannedItem";
import { NewPlannedItem } from "./NewPlannedItem";
import { Amount } from "components";

interface PlannedItemsTableProps {
    plan?: ForecastPlan;
    currencyCode: string;
    progress?: PlannedItemProgress[];
}

export const PlannedItemsTable: React.FC<PlannedItemsTableProps> = ({ plan, currencyCode, progress }) => {

    const items = plan?.plannedItems ?? [];
    const planId = plan?.id;

    const incomeItems = items.filter(i => i.itemType === "Income");
    const expenseItems = items.filter(i => i.itemType === "Expense");

    const progressById = new Map((progress ?? []).map(p => [p.plannedItemId, p]));

    return (
        <>
            <PlannedItemsSection planId={planId} title="Planned Income" items={incomeItems} itemType="Income" currencyCode={currencyCode} progressById={progressById} />
            <PlannedItemsSection planId={planId} title="Planned Expenses" items={expenseItems} itemType="Expense" currencyCode={currencyCode} progressById={progressById} />
        </>
    );
};

interface PlannedItemsSectionProps {
    planId: string;
    title: string;
    items: PlannedItem[];
    itemType: "Income" | "Expense";
    currencyCode: string;
    progressById: Map<string, PlannedItemProgress>;
}

const PlannedItemsSection: React.FC<PlannedItemsSectionProps> = ({ planId, title, items, itemType, currencyCode, progressById }) => {
    return (
        <SectionTable header={title} striped>
            <thead>
                <tr>
                    <th className="column-20">Name</th>
                    <th className="column-10">Amount</th>
                    <th className="column-15">Tag</th>
                    <th className="column-15">Spent</th>
                    <th className="column-12">Start Date</th>
                    <th className="column-12">End Date</th>
                    <th className="column-12">Frequency</th>
                    <th className="row-action"></th>
                </tr>
            </thead>
            <tbody>
                {items.map((item) => (
                    <PlannedItemRow key={item.id} planId={planId} item={item} currencyCode={currencyCode} progress={progressById.get(item.id)} />
                ))}
                <NewPlannedItem planId={planId} itemType={itemType} />
            </tbody>
            <tfoot>
                <tr>
                    <td>Total</td>
                    <td className="amount"><Amount amount={items.reduce((sum, i) => sum + (i.isIncluded ? i.amount : 0), 0)} currencyCode={currencyCode} minus /></td>
                    <td colSpan={6}></td>
                </tr>
            </tfoot>
        </SectionTable>
    );
};

interface PlannedItemRowProps {
    planId: string;
    item: PlannedItem;
    currencyCode: string;
    progress?: PlannedItemProgress;
}

// What has actually been spent against an item, and what that says about it. An untagged item shows
// nothing, because nothing can be attributed to it: this is how the author sees which figures are
// being measured against reality and which are still only guesses.
const SpentCell: React.FC<{ progress?: PlannedItemProgress; currencyCode: string }> = ({ progress, currencyCode }) => {
    if (!progress?.isMatched) {
        return <td className="planned-item-spent untracked">not tracked</td>;
    }

    const overspent = progress.actualToDate > progress.plannedTotal;
    const shortOfPlan = progress.isClosed && progress.actualToDate < progress.plannedTotal;

    return (
        <td className="planned-item-spent amount">
            <Amount amount={progress.actualToDate} currencyCode={currencyCode} />
            {overspent && <span className="planned-item-note over">over</span>}
            {shortOfPlan && progress.actualToDate === 0 && <span className="planned-item-note unseen">not seen</span>}
            {shortOfPlan && progress.actualToDate > 0 && <span className="planned-item-note under">came in under</span>}
            {!progress.isClosed && progress.remaining > 0 && progress.actualToDate > 0 && (
                <span className="planned-item-note remaining">
                    <Amount amount={progress.remaining} currencyCode={currencyCode} /> to go
                </span>
            )}
        </td>
    );
};

const PlannedItemRow: React.FC<PlannedItemRowProps> = ({ planId, item: propItem, currencyCode, progress }) => {
    const [item, setItem] = useUpdatingState(propItem);
    const { update } = useUpdatePlannedItem();
    const { data: tags } = useTags();
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
        <tr className={!item.isIncluded ? "excluded" : ""}>
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
            <td className="planned-item-tag">
                <ComboBox
                    clearable
                    placeholder="Untagged"
                    items={tags ?? []}
                    selectedItems={(tags ?? []).filter(t => t.id === item.tagId)}
                    labelField={(t: Tag) => t?.name}
                    valueField={(t: Tag) => String(t?.id)}
                    onChange={(selected: Tag[]) => handleUpdate({ tagId: selected[0]?.id ?? undefined })}
                />
            </td>
            <SpentCell progress={progress} currencyCode={currencyCode} />
            <EditColumn
                type="date"
                value={getDateValue()}
                onChange={(v) => handleDateChange(v.value)}
            >
                {formatStartDateDisplay()}
            </EditColumn>
            {item.dateMode === "FixedDate" ? (
                <td className="no-value">-</td>
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
            <td className="row-action">
                <DeleteIcon onClick={handleDelete} />
            </td>
        </tr>
    );
};
