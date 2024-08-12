import { SectionTable } from "@andrewmclachlan/mooapp";
import * as Models from "models";
import { BudgetLine } from "./BudgetLine";
import { NewBudgetLine } from "./NewBudgetLine";
import { numberOfMonths } from "helpers/dateFns";

export const BudgetTable: React.FC<BudgetTableProps> = ({ title, type, year, lines = [] }) => {
    return (
        <SectionTable striped className="budget-list" title={title}>
            <thead>
                <tr>
                    <th className="column-20">Tag</th>
                    <th className="column-50">Notes</th>
                    <th className="column-5">Amount</th>
                    <th className="column-20">When</th>
                    <th className="column-5"></th>
                </tr>
            </thead>
            <tbody>
                {lines.map((b) =>
                    <BudgetLine year={year} budgetLine={b} key={b.id} />
                )}

                <NewBudgetLine year={year} type={type} />
            </tbody>
            <tfoot>
                <tr>
                    <td colSpan={2}>Monthly Average</td>
                    <td colSpan={3}>{(lines.map(b => b.amount * numberOfMonths(b.month ?? 0)).reduce((total, current) => total + current, 0) / 12).toFixed(2)}</td>
                </tr>
            </tfoot>
        </SectionTable>
    );
}

export interface BudgetTableProps {
    type: Models.BudgetLineType;
    lines?: Models.BudgetLine[];
    title: string;
    year: number;
}
