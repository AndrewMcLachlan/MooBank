import { SaveIcon, Input, ComboBox } from "@andrewmclachlan/moo-ds";
import { format } from "date-fns";
import type { PlannedItemDateMode, PlannedItemType, ScheduleFrequency, Tag } from "api/types.gen";
import { useState } from "react";
import { useTags } from "hooks/useTags";
import { useCreatePlannedItem } from "../-hooks/useCreatePlannedItem";

interface NewPlannedItemProps {
    planId: string;
    itemType: PlannedItemType;
    /// Whether the section shows the tag and spent columns, so this row lines up with them.
    tracked: boolean;
}

export const NewPlannedItem: React.FC<NewPlannedItemProps> = ({ planId, itemType, tracked }) => {
    const { create, isPending } = useCreatePlannedItem();
    const { data: tags } = useTags();

    const [name, setName] = useState("");
    const [amount, setAmount] = useState(0);
    const [dateMode, setDateMode] = useState<PlannedItemDateMode>("Schedule");
    const [startDate, setStartDate] = useState(format(new Date(), "yyyy-MM-dd"));
    const [scheduleFrequency, setScheduleFrequency] = useState<ScheduleFrequency>("Monthly");
    const [endDate, setEndDate] = useState("");
    const [notes, setNotes] = useState("");
    const [tagId, setTagId] = useState<number | undefined>(undefined);

    const handleAdd = () => {
        if (!name || amount <= 0) return;

        create(planId, {
            itemType,
            name,
            amount,
            isIncluded: true,
            dateMode,
            fixedDate: dateMode === "FixedDate" ? startDate : undefined,
            scheduleFrequency: dateMode === "Schedule" ? scheduleFrequency : undefined,
            scheduleAnchorDate: dateMode === "Schedule" ? startDate : undefined,
            scheduleInterval: dateMode === "Schedule" ? 1 : undefined,
            scheduleEndDate: dateMode === "Schedule" && endDate ? endDate : undefined,
            notes: notes || undefined,
            tagId,
        });

        // Reset form
        setName("");
        setAmount(0);
        setEndDate("");
        setNotes("");
        setTagId(undefined);
    };

    return (
        <tr className="new-planned-item">
            <td>
                <Input
                    type="text"
                    placeholder="Item name"
                    value={name}
                    onChange={(e) => setName(e.target.value)}
                />
            </td>
            <td>
                <Input
                    type="number"
                    min={0}
                    step={0.01}
                    value={amount || ""}
                    onChange={(e) => setAmount(parseFloat(e.target.value) || 0)}
                />
            </td>
            {tracked && <td className="planned-item-tag">
                <ComboBox
                    clearable
                    placeholder="Untagged"
                    items={tags ?? []}
                    selectedItems={(tags ?? []).filter(t => t.id === tagId)}
                    labelField={(t: Tag) => t?.name}
                    valueField={(t: Tag) => String(t?.id)}
                    onChange={(selected: Tag[]) => setTagId(selected[0]?.id ?? undefined)}
                />
            </td>}
            {tracked && <td />}
            <td>
                <Input
                    type="date"
                    value={startDate}
                    onChange={(e) => setStartDate(e.target.value)}
                />
            </td>
            <td>
                {dateMode === "FixedDate" ? (
                    <span className="no-value">-</span>
                ) : (
                    <Input
                        type="date"
                        placeholder="Ongoing"
                        value={endDate}
                        onChange={(e) => setEndDate(e.target.value)}
                    />
                )}
            </td>
            <td>
                <Input.Select
                    value={dateMode === "FixedDate" ? "FixedDate" : scheduleFrequency}
                    onChange={(e) => {
                        if (e.target.value === "FixedDate") {
                            setDateMode("FixedDate");
                        } else {
                            setDateMode("Schedule");
                            setScheduleFrequency(e.target.value as ScheduleFrequency);
                        }
                    }}
                >
                    <option value="FixedDate">One-time</option>
                    <option value="Weekly">Weekly</option>
                    <option value="Fortnightly">Fortnightly</option>
                    <option value="Monthly">Monthly</option>
                    <option value="Yearly">Yearly</option>
                </Input.Select>
            </td>
            <td>
                <Input
                    type="text"
                    placeholder="Notes (optional)"
                    value={notes}
                    onChange={(e) => setNotes(e.target.value)}
                />
            </td>
            <td className="row-action">
                {(isPending || !name || amount <= 0) ? null : <SaveIcon onClick={handleAdd} />}
            </td>
        </tr>
    );
};
