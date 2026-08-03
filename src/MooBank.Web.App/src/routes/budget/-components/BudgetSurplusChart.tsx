import { Bar } from "react-chartjs-2";
import { Section } from "@andrewmclachlan/moo-ds";
import type { BudgetMonth } from "api/types.gen";
import { useChartColours } from "utils/chartColours";
import { budgetSurplusChartData, budgetSurplusChartOptions } from "../-utils/budgetYearChart";

export const BudgetSurplusChart: React.FC<BudgetSurplusChartProps> = ({ months }) => {

    const colours = useChartColours();

    return (
        <Section header="Surplus by Month" headerSize={3}>
            <div className="budget-year-chart">
                <Bar id="budget-surplus" data={budgetSurplusChartData(months ?? [], colours)} options={budgetSurplusChartOptions(colours)} />
            </div>
        </Section>
    );
};

export interface BudgetSurplusChartProps {
    months?: BudgetMonth[];
}
