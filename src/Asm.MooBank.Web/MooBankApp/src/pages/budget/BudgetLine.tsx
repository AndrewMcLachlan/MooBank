import { ClickableIcon, EditColumn, useIdParams } from "@andrewmclachlan/mooapp";
import { MonthSelector, TransactionTagPanel } from "components";
import * as Models from "models";
import { useDeleteBudgetLine, useUpdateBudgetLine } from "services/BudgetService";

export const BudgetLine: React.FC<BudgetLineProps> = ({accountId, year, budgetLine}) => {

    const updateBudgetLine = useUpdateBudgetLine();
    const deleteBudgetLine = useDeleteBudgetLine();

    const onDelete = () => {
        deleteBudgetLine.deleteBudgetLine( accountId, year, budgetLine.id);
    }

    return (
        <tr key={budgetLine.id}>
            <td>{budgetLine.name}</td>
            <td><EditColumn value={budgetLine.amount.toFixed(2).toString()} onChange={(v) => updateBudgetLine.update(accountId, year, {...budgetLine, amount: Number(v)})}/></td>
            <td><MonthSelector value={budgetLine.month} onChange={(v) => updateBudgetLine.update(accountId, year, {...budgetLine, month: v})} /></td>
            <td><ClickableIcon icon="trash-alt" onClick={onDelete} /></td>
        </tr>
    );
}

export interface BudgetLineProps {
    accountId: string;
    year: number;
    budgetLine: Models.BudgetLine;
}