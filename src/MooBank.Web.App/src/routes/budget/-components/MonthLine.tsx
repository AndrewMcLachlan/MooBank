import type { BudgetMonth } from "api/types.gen";
import { format } from "date-fns/format";
import { Amount } from "components/Amount";

export const MonthLine: React.FC<MonthLineProps> = ({ month }) => (
    <tr>
        <td>{format(new Date(2000, month.month, 1), "MMMM")}</td>
        <td><Amount amount={month.income} minus /></td>
        <td><Amount amount={month.expenses} minus /></td>
        <td><Amount amount={month.remainder} minus /></td>
    </tr>

);

export interface MonthLineProps {
    month: BudgetMonth;
}
