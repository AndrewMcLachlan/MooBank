import { ClickableIcon, emptyGuid, useIdParams } from "@andrewmclachlan/mooapp";
import Select from "react-select";

import { MonthSelector, TransactionTagPanel } from "components";
import { Tag } from "models";
import { useEffect, useState } from "react";
import { Form } from "react-bootstrap";
import { useTags } from "services";
import { useGetTagValue, useCreateBudgetLine } from "services/BudgetService";

export const NewBudgetLine: React.FC<NewBudgetLineProps> = (props) => {

    const createBudget = useCreateBudgetLine();
    const allTags = useTags();

    const [amount, setAmount] = useState(0);
    const [tag, setTag] = useState<Tag>(null);
    const [month, setMonth] = useState(4095);

    const { data: tagValue } = useGetTagValue(tag?.id);

    useEffect(() => {
        if (!tagValue || !tag) return;

        setAmount(Math.abs(tagValue));
    }, [tagValue]);

    const add = () => {
        createBudget.create(props.year, { amount, tagId: tag?.id, name: tag?.name, id: emptyGuid, income: props.income!, month });
        setTag(null);
        setAmount(0);
    };

    return (
        <tr>
            <td className="column-30"><Select<Tag> value={tag} onChange={(t: Tag) => setTag(t)} options={allTags.data ?? []} getOptionLabel={(t) => t.name} getOptionValue={(t) => t.id?.toString()} className="react-select" classNamePrefix="react-select" /></td>
            <td><Form.Control type="number" min={0} value={amount} onChange={(e) => setAmount((e.currentTarget as any).valueAsNumber)} /></td>
            <MonthSelector as="td" className="column-30" value={month} onChange={setMonth} />
            <td className="column-5"><ClickableIcon icon="check-circle" title="Save" size="xl" onClick={add} /></td>
        </tr>
    );
}

NewBudgetLine.defaultProps = {
    income: false,
}

export interface NewBudgetLineProps {
    year: number;
    income?: boolean;
}