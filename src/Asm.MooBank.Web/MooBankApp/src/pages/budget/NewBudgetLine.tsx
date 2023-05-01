import { ClickableIcon, emptyGuid } from "@andrewmclachlan/mooapp";
import Select from "react-select";

import { TransactionTagPanel } from "components";
import { TransactionTag } from "models";
import { useState } from "react";
import { Form } from "react-bootstrap";
import { useTags } from "services";
import { useCreateBudget } from "services/BudgetService";

export const NewBudgetLine: React.FC<NewBudgetLineProps> = (props) => {

    const createBudget = useCreateBudget();
    const allTags = useTags();

    const [amount, setAmount] = useState(0);
    const [tag, setTag] = useState<TransactionTag>(null);
    

    const add = () => {
        createBudget.create(props.accountId, { amount, tagId: tag?.id, name: tag?.name, id: emptyGuid, income: props.income! });
    }

    return (
        <tr>
            <td className="column-30"><Select<TransactionTag> value={tag} onChange={(t: TransactionTag) => setTag(t)} options={allTags.data ?? []} getOptionLabel={(t) => t.name} getOptionValue={(t) => t.id?.toString()} className="react-select" classNamePrefix="react-select" /></td>
            <td><Form.Control type="number" min={0} value={amount} onChange={(e) => setAmount((e.currentTarget as any).valueAsNumber)} /></td>
            <td className="column-5"><ClickableIcon icon="check-circle" title="Save" size="xl" onClick={add} /></td>
        </tr>
    );
}

NewBudgetLine.defaultProps = {
    income: false,
}

export interface NewBudgetLineProps {
    accountId: string;
    income?: boolean;
}