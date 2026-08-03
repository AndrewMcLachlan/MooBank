import { SectionTable } from "@andrewmclachlan/moo-ds";
import type { BudgetLine as BudgetLineModel, BudgetLineType } from "api/types.gen";
import { BudgetLine } from "./BudgetLine";
import { NewBudgetLine } from "./NewBudgetLine";
import { numberOfMonths } from "utils/dateFns";
import { useTags } from "hooks/useTags";
import { Amount } from "components/Amount";

export const BudgetTable: React.FC<BudgetTableProps> = ({ title, type, year, lines = [] }) => {

    const { data: tags } = useTags();
    const colourFor = (tagId: number) => tags?.find(t => t.id === tagId)?.colour as string | undefined;

    return (
        <SectionTable striped className="budget-list" header={title}>
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
                    <BudgetLine year={year} budgetLine={b} colour={colourFor(b.tagId)} key={b.id} />
                )}

                <NewBudgetLine year={year} type={type} />
            </tbody>
            <tfoot>
                <tr>
                    <td colSpan={2}>Monthly Average</td>
                    <td colSpan={3}><Amount amount={lines.map(b => b.amount * numberOfMonths(b.month ?? 0)).reduce((total, current) => total + current, 0) / 12} minus /></td>
                </tr>
            </tfoot>
        </SectionTable>
    );
}

export interface BudgetTableProps {
    type: BudgetLineType;
    lines?: BudgetLineModel[];
    /** Omitted when the table sits in a tab that already names it. */
    title?: string;
    year: number;
}
