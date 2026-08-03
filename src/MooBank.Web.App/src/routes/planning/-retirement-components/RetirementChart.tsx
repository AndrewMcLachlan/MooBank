import { Line } from "react-chartjs-2";
import { Section } from "@andrewmclachlan/moo-ds";
import type { RetirementProjectionYear } from "api/types.gen";
import { useChartColours } from "utils/chartColours";
import { retirementChartData, retirementChartOptions } from "../-retirement-utils/retirementChart";
import { ChartSkeleton } from "../-components/ChartSkeleton";

interface RetirementChartProps {
    years: RetirementProjectionYear[];
    currencyCode: string;
    /** The balance below which the Age Pension starts, marked as a straight line. */
    pensionStartsBelow?: number;
    loading?: boolean;
}

export const RetirementChart: React.FC<RetirementChartProps> = ({ years, currencyCode, pensionStartsBelow, loading }) => {

    const colours = useChartColours();

    if (loading) return <ChartSkeleton header="Projected Balance" canvasClassName="retirement-chart-canvas" />;

    if (years.length === 0) return null;

    return (
        <Section header="Projected Balance">
            <div className="retirement-chart-canvas">
                <Line data={retirementChartData(years, colours, pensionStartsBelow)} options={retirementChartOptions(currencyCode, colours)} />
            </div>
        </Section>
    );
};
