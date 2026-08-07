import { Line } from "react-chartjs-2";
import { Section } from "@andrewmclachlan/moo-ds";
import type { BudgetMonth } from "api/types.gen";
import { useChartColours } from "utils/chartColours";
import { budgetCumulativeChartData, budgetCumulativeChartOptions } from "../-utils/budgetYearChart";

export const BudgetCumulativeChart: React.FC<BudgetCumulativeChartProps> = ({ months }) => {

    const colours = useChartColours();

    return (
        <Section header="Cumulative Surplus" headerSize={3}>
            <div className="budget-year-chart">
                <Line id="budget-cumulative" data={budgetCumulativeChartData(months ?? [])} options={budgetCumulativeChartOptions(colours)} />
            </div>
        </Section>
    );
};

export interface BudgetCumulativeChartProps {
    months?: BudgetMonth[];
}
