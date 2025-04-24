import * as Models from "models";
import { format } from "date-fns/format";

export const MonthLine: React.FC<MonthLineProps> = ({ month }) => (
    <tr>
        <td>{format(new Date(2000, month.month, 1), "MMMM")}</td>
        <td><span className="amount">{month.income.toFixed(2)}</span></td>
        <td><span className="amount">{month.expenses.toFixed(2)}</span></td>
        <td><span className="amount">{month.remainder.toFixed(2)}</span></td>
    </tr>

);

export interface MonthLineProps {
    month: Models.BudgetMonth;
}
