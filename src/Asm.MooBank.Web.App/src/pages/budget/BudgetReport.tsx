import React, { useEffect, useRef, useState } from "react";

import { Bar, getElementAtEvent } from "react-chartjs-2";
import { Chart as ChartJS, ChartData, registerables } from "chart.js";
import chartTrendline from "chartjs-plugin-trendline";

import { useChartColours } from "helpers/chartColours";
import { Section, useLocalStorage } from "@andrewmclachlan/mooapp";
import { Col, Form, Row } from "react-bootstrap";
import { useBudgetReport, useBudgetYears } from "services/BudgetService";
import { BudgetPage } from "./BudgetPage";
import { BudgetReportYear } from "./BudgetReportYear";
import { BudgetReportTags } from "./BudgetReportTags";
import { useBudgetYear } from "hooks/useBudgetYear";

ChartJS.register(...registerables);
ChartJS.register(chartTrendline);

export const BudgetReport: React.FC = () => {

    const colours = useChartColours();

    const next5Years = [...Array(5).keys()].map(i => new Date().getFullYear() + i);

    const [year, setYear] = useBudgetYear();
    const [selectableYears, setSelectableYears] = useState(next5Years);
    const [month, setMonth] = useState<number | undefined>(undefined);

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
            {month === undefined && <BudgetReportYear year={year} onDrilldown={setMonth} />}
            {month !== undefined && <BudgetReportTags year={year} month={month} />}
        </BudgetPage>
    );
};
