import { emptyGuid, SaveIcon } from "@andrewmclachlan/moo-ds";
import { format } from "date-fns";
import { PlannedItemDateMode, PlannedItemType, ScheduleFrequency } from "models";
import { useState } from "react";
import { Form, Input } from "@andrewmclachlan/moo-ds";
import { useCreatePlannedItem } from "services/ForecastService";

interface NewPlannedItemProps {
    planId: string;
    itemType: PlannedItemType;
}

export const NewPlannedItem: React.FC<NewPlannedItemProps> = ({ planId, itemType }) => {
    const { create, isPending } = useCreatePlannedItem();

    const [name, setName] = useState("");
    const [amount, setAmount] = useState(0);
    const [dateMode, setDateMode] = useState<PlannedItemDateMode>("Schedule");
    const [fixedDate, setFixedDate] = useState(format(new Date(), "yyyy-MM-dd"));
    const [scheduleFrequency, setScheduleFrequency] = useState<ScheduleFrequency>("Monthly");
    const [scheduleAnchorDate, setScheduleAnchorDate] = useState(format(new Date(), "yyyy-MM-dd"));
    const [scheduleEndDate, setScheduleEndDate] = useState("");
    const [notes, setNotes] = useState("");

    const handleAdd = () => {
        if (!name || amount <= 0) return;

        create(planId, {
            itemType,
            name,
            amount,
            isIncluded: true,
            dateMode,
            fixedDate: dateMode === "FixedDate" ? fixedDate : undefined,
            scheduleFrequency: dateMode === "Schedule" ? scheduleFrequency : undefined,
            scheduleAnchorDate: dateMode === "Schedule" ? scheduleAnchorDate : undefined,
            scheduleInterval: dateMode === "Schedule" ? 1 : undefined,
            scheduleEndDate: dateMode === "Schedule" && scheduleEndDate ? scheduleEndDate : undefined,
            notes: notes || undefined
        });

        // Reset form
        setName("");
        setAmount(0);
        setScheduleEndDate("");
        setNotes("");
    };

    const formatWhenDisplay = () => {
        switch (dateMode) {
            case "FixedDate":
                return fixedDate;
            case "Schedule":
                return scheduleAnchorDate;
            default:
                return "-";
        }
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
            <td>
                <Input
                    type="date"
                    value={dateMode === "FixedDate" ? fixedDate : scheduleAnchorDate}
                    onChange={(e) => {
                        if (dateMode === "FixedDate") {
                            setFixedDate(e.target.value);
                        } else {
                            setScheduleAnchorDate(e.target.value);
                        }
                    }}
                />
            </td>
            <td>
                {dateMode === "FixedDate" ? (
                    <span className="text-muted">-</span>
                ) : (
                    <Input
                        type="date"
                        placeholder="Ongoing"
                        value={scheduleEndDate}
                        onChange={(e) => setScheduleEndDate(e.target.value)}
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
