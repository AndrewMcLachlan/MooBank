import { Line } from "react-chartjs-2";
import { Section } from "@andrewmclachlan/moo-ds";
import type { RetirementProjectionYear } from "api/types.gen";
import { useChartColours } from "utils/chartColours";
import { retirementChartData, retirementChartOptions } from "../-retirement-utils/retirementChart";

interface RetirementChartProps {
    years: RetirementProjectionYear[];
    currencyCode: string;
    /** The balance below which the Age Pension starts, marked as a straight line. */
    pensionStartsBelow?: number;
}

export const RetirementChart: React.FC<RetirementChartProps> = ({ years, currencyCode, pensionStartsBelow }) => {

    const colours = useChartColours();

    if (years.length === 0) return null;

    return (
        <Section header="Projected Balance">
            <div className="retirement-chart-canvas">
                <Line data={retirementChartData(years, colours, pensionStartsBelow)} options={retirementChartOptions(currencyCode, colours)} />
            </div>
        </Section>
    );
};
