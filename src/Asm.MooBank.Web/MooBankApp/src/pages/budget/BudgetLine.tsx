import { MonthSelector, TransactionTagPanel } from "components";
import * as Models from "models";
import { useDeleteBudgetLine, useUpdateBudget } from "services/BudgetService";

export const BudgetLine: React.FC<BudgetLineProps> = (props) => {

    const updateBudget = useUpdateBudget();
    const deleteBudget = useDeleteBudgetLine();

    return (
        <tr key={props.budgetLine.id}>
            <td><TransactionTagPanel value={props.budgetLine.tagId} /></td>
            <td>{props.budgetLine.amount}</td>
            <td><MonthSelector /></td>
        </tr>
    );
}

export interface BudgetLineProps {
    budgetLine: Models.BudgetLine;
}