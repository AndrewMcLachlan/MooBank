import React, { useEffect, useRef, useState } from "react";

import { Bar } from "react-chartjs-2";
import { Chart as ChartJS, ChartData, registerables } from "chart.js";
import chartTrendline from "chartjs-plugin-trendline";

import { useChartColours } from "helpers/chartColours";
import { Section } from "@andrewmclachlan/mooapp";
import { Col, Form, Row } from "react-bootstrap";
import { useBudgetReport, useBudgetYears } from "services/BudgetService";
import { BudgetPage } from "./BudgetPage";

ChartJS.register(...registerables);
ChartJS.register(chartTrendline);

export const BudgetReport: React.FC = () => {

    const colours = useChartColours();
    const chartRef = useRef();

    const next5Years = [...Array(5).keys()].map(i => new Date().getFullYear() + i);

    const [year, setYear] = useState(new Date().getFullYear());
    const [selectableYears, setSelectableYears] = useState(next5Years);

    const { data: budgetYears } = useBudgetYears();

    useEffect(() => {
        if (!budgetYears) return;

        const newYears = budgetYears;

        next5Years.forEach(y => { if (!newYears.includes(y)) newYears.push(y); });

        setSelectableYears(newYears);

    }, [budgetYears]);

    const report = useBudgetReport(year);

    const dataset: ChartData<"bar", number[], string> = {
        labels: ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"],

        datasets: [{
            label: "Budgeted",
            data: report.data?.items.map(i => i.budgetedAmount) ?? [0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
            backgroundColor: colours.income,
        }, {
            label: "Actual",
            data: report.data?.items.map(i => i.actual ?? 0) ?? [0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
            backgroundColor: colours.expenses,
        }]
    };

    return (
        <BudgetPage title="Budget Report">
            <Section>
                <Row>
                    <Col>
                        <Form.Select value={year} onChange={e => setYear(Number(e.currentTarget.value))}>
                            {selectableYears.map((y) =>
                                <option value={y} key={y}>{y}</option>
                            )}
                        </Form.Select>
                    </Col>
                </Row>
            </Section>
            <Section className="report" style={{ width: "2000px ! important"}}>
                <h3>Budget Report</h3>
                <Bar id="budget-report" ref={chartRef} data={dataset} options={{
                    plugins: {
                        tooltip: {
                            mode: "point",
                            intersect: false,
                        } as any,
                    },
                    hover: {
                        //mode: "point",
                        //intersect: true,
                    },
                    scales: {
                        x: {
                            stacked: false
                        }
                    }
                }}
                   /* onClick={(e) => {
                        var elements = getElementAtEvent(chartRef.current!, e);
                        if (elements.length !== 1) return;
                        if (!report.data!.tags[elements[0].index].hasChildren) return;
                        navigate(`/accounts/${accountId}/reports/breakdown/${report.data!.tags[elements[0].index].tagId}`);
                    }}*/
                />
            </Section>
        </BudgetPage>
    );

}
