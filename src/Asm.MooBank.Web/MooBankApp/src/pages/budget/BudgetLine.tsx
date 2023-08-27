import { ClickableIcon, EditColumn } from "@andrewmclachlan/mooapp";
import { MonthSelector } from "components";
import * as Models from "models";
import { useDeleteBudgetLine, useUpdateBudgetLine } from "services/BudgetService";

export const BudgetLine: React.FC<BudgetLineProps> = ({year, budgetLine}) => {

    const updateBudgetLine = useUpdateBudgetLine();
    const deleteBudgetLine = useDeleteBudgetLine();

    const onDelete = () => {
        deleteBudgetLine.deleteBudgetLine( year, budgetLine.id);
    }

    return (
        <tr key={budgetLine.id}>
            <td>{budgetLine.name}</td>
            <EditColumn value={budgetLine.amount.toFixed(2).toString()} onChange={(v) => updateBudgetLine.update(year, {...budgetLine, amount: Number(v)})}/>
            <td><MonthSelector value={budgetLine.month} onChange={(v) => updateBudgetLine.update(year, {...budgetLine, month: v})} /></td>
            <td><ClickableIcon icon="trash-alt" onClick={onDelete} /></td>
        </tr>
    );
}

export interface BudgetLineProps {
    year: number;
    budgetLine: Models.BudgetLine;
}