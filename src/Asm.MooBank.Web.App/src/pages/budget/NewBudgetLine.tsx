import { emptyGuid } from "@andrewmclachlan/mooapp";
import Select from "react-select";

import { MonthSelector } from "components";
import { SaveIcon } from "components/SaveIcon";
import { BudgetLineType, Tag } from "models";
import { useEffect, useState } from "react";
import { Form } from "react-bootstrap";
import { useTags } from "services";
import { useCreateBudgetLine, useGetTagValue } from "services/BudgetService";

export const NewBudgetLine: React.FC<NewBudgetLineProps> = (props) => {

    const createBudget = useCreateBudgetLine();
    const allTags = useTags();

    const [amount, setAmount] = useState(0);
    const [tag, setTag] = useState<Tag>(null);
    const [notes, setNotes] = useState<string>("");
    const [month, setMonth] = useState(4095);

    const { data: tagValue } = useGetTagValue(tag?.id);

    useEffect(() => {
        if (!tagValue || !tag) return;

        setAmount(Math.abs(tagValue));
    }, [tagValue]);

    const add = () => {
        createBudget(props.year, { amount, tagId: tag?.id, name: tag?.name, notes: notes, id: emptyGuid, type: props.type, month });
        setTag(null);
        setAmount(0);
        setNotes("");
    };

    return (
        <tr>
            <td><Select<Tag> value={tag} onChange={(t: Tag) => setTag(t)} options={allTags.data ?? []} getOptionLabel={(t) => t.name} getOptionValue={(t) => t.id?.toString()} className="react-select" classNamePrefix="react-select" /></td>
            <td><Form.Control value={notes} onChange={(e) => setNotes(e.currentTarget.value)} /></td>
            <td><Form.Control type="number" min={0} value={amount} onChange={(e) => setAmount((e.currentTarget as any).valueAsNumber)} /></td>
            <MonthSelector as="td" value={month} onChange={setMonth} />
            <td className="row-action"><SaveIcon onClick={add} /></td>
        </tr>
    );
}

export interface NewBudgetLineProps {
    year: number;
    type: BudgetLineType;
}
