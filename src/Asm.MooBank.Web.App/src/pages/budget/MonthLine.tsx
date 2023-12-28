import * as Models from "models";
import { format } from "date-fns/format";

export const MonthLine: React.FC<MonthLineProps> = ({ month }) => (
    <tr>
        <td>{format(new Date(2000, month.month, 1), "MMMM")}</td>
        <td>{month.income.toFixed(2)}</td>
        <td>{month.expenses.toFixed(2)}</td>
        <td>{month.remainder.toFixed(2)}</td>
    </tr>

);

export interface MonthLineProps {
    month: Models.BudgetMonth;
}
