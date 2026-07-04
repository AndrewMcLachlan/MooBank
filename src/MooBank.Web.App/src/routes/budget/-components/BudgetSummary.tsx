import type { Budget } from "api/types.gen";
import { Amount } from "components/Amount";

export const BudgetSummary: React.FC<BudgetSummaryProps> = ({ budget }) => {

    const income = budget?.months.reduce((total, m) => total + m.income, 0) ?? 0;
    const expenses = budget?.months.reduce((total, m) => total + m.expenses, 0) ?? 0;
    const surplus = income - expenses;

    return (
        <section className="budget-summary" aria-label="Annual budget summary">
            <div className="kpi">
                <span className="kpi-label">Income</span>
                <span className="kpi-value income"><Amount amount={income} /></span>
            </div>
            <div className="kpi">
                <span className="kpi-label">Expenses</span>
                <span className="kpi-value expense"><Amount amount={expenses} /></span>
            </div>
            <div className="kpi">
                <span className="kpi-label">Surplus</span>
                <span className="kpi-value"><Amount amount={surplus} positiveColour negativeColour minus /></span>
            </div>
        </section>
    );
};

export interface BudgetSummaryProps {
    budget?: Budget;
}
