import type { Budget } from "api/types.gen";
import { Amount } from "components/Amount";
import { Kpi } from "@andrewmclachlan/moo-ds";

export const BudgetSummary: React.FC<BudgetSummaryProps> = ({ budget }) => {

    const income = budget?.months.reduce((total, m) => total + m.income, 0) ?? 0;
    const expenses = budget?.months.reduce((total, m) => total + m.expenses, 0) ?? 0;
    const surplus = income - expenses;

    return (
        <section className="budget-summary" aria-label="Annual budget summary">
            <Kpi label="Income" tone="income">
                <Kpi.Value className="income"><Amount amount={income} /></Kpi.Value>
            </Kpi>
            <Kpi label="Expenses" tone="expense">
                <Kpi.Value className="expense"><Amount amount={expenses} /></Kpi.Value>
            </Kpi>
            {/* The surplus takes its accent from the result: living within the budget reads as
                income, spending beyond it as an expense. Breaking exactly even is not overspending. */}
            <Kpi label="Surplus" tone={surplus < 0 ? "expense" : "income"}>
                <Kpi.Value><Amount amount={surplus} positiveColour negativeColour minus /></Kpi.Value>
            </Kpi>
        </section>
    );
};

export interface BudgetSummaryProps {
    budget?: Budget;
}
