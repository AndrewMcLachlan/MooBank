import { Bar } from "react-chartjs-2";
import { Section } from "@andrewmclachlan/moo-ds";
import type { RetirementProjectionYear } from "api/types.gen";
import { useChartColours } from "utils/chartColours";
import { hasRetirementIncome, retirementIncomeChartData, retirementIncomeChartOptions } from "../-retirement-utils/retirementIncomeChart";

interface RetirementIncomeChartProps {
    years: RetirementProjectionYear[];
    currencyCode: string;
}

/**
 * What the household actually lives on each year of retirement, stacked by whose balance funds it.
 *
 * The balance chart answers "how much will we have"; this answers "what do we get to spend, and for
 * how long" — which is the question a retirement plan is really asked.
 */
export const RetirementIncomeChart: React.FC<RetirementIncomeChartProps> = ({ years, currencyCode }) => {

    const colours = useChartColours();

    if (!hasRetirementIncome(years)) return null;

    return (
        <Section header="Retirement Income">
            <div className="retirement-chart-canvas">
                <Bar data={retirementIncomeChartData(years)} options={retirementIncomeChartOptions(currencyCode, colours, years)} />
            </div>
        </Section>
    );
};
