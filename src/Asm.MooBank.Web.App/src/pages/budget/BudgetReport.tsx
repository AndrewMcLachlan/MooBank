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
import { useNavigate, useParams } from "react-router-dom";

ChartJS.register(...registerables);
ChartJS.register(chartTrendline);

export const BudgetReport: React.FC = () => {

    const colours = useChartColours();

    const next5Years = [...Array(5).keys()].map(i => new Date().getFullYear() + i);

    const { month: pathMonth } = useParams<{month?: string}>();

    const [year, setYear] = useBudgetYear();
    const [selectableYears, setSelectableYears] = useState(next5Years);
    const [month, setMonth] = useState<number | undefined>(pathMonth ? Number(pathMonth) : undefined);

    const { data: budgetYears } = useBudgetYears();

    const navigate = useNavigate();

    const setMonthAndPath = (month: number ) =>{
        navigate(`/budget/report/${month}`);
        setMonth(month);
    };

    useEffect(() => {
        setMonth(pathMonth ? Number(pathMonth) : undefined)
    }, [pathMonth]);

    useEffect(() => {
        if (!budgetYears) return;

        const newYears = budgetYears;

        next5Years.forEach(y => { if (!newYears.includes(y)) newYears.push(y); });

        setSelectableYears(newYears);

    }, [budgetYears]);

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
            {month === undefined && <BudgetReportYear year={year} onDrilldown={setMonthAndPath} />}
            {month !== undefined && <BudgetReportTags year={year} month={month} />}
        </BudgetPage>
    );
};
