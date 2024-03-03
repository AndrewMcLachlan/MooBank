import React, { useEffect, useRef, useState } from "react";

import { Chart as ChartJS, ChartData, registerables } from "chart.js";
import chartTrendline from "chartjs-plugin-trendline";

import { Section, useUpdatingState } from "@andrewmclachlan/mooapp";
import { Col, Form, Row } from "react-bootstrap";
import { useBudgetYears } from "services/BudgetService";
import { BudgetPage } from "./BudgetPage";
import { BudgetReportYear } from "./BudgetReportYear";
import { BudgetReportTags } from "./BudgetReportTags";
import { useBudgetYear } from "hooks/useBudgetYear";
import { useNavigate, useParams } from "react-router-dom";

ChartJS.register(...registerables);
ChartJS.register(chartTrendline);

export const BudgetReport: React.FC = () => {

    const next5Years = [...Array(5).keys()].map(i => new Date().getFullYear() + i);

    const { year: pathYear, month: pathMonth } = useParams<{ year?: string, month?: string }>();

    const [year, setYear] = useBudgetYear();
    const [selectableYears, setSelectableYears] = useState(next5Years);
    const [month, setMonth] = useUpdatingState<number | undefined>(pathMonth ? Number(pathMonth) : undefined);

    const { data: budgetYears } = useBudgetYears();

    const navigate = useNavigate();

    const setMonthAndPath = (month: number) => {
        navigate(`/budget/report/${year}/${month}`);
    };

    const yearChange = (value: number) => {
        setYear(value);
        navigate(`/budget/report/${value}`);
    }

    useEffect(() => {

        if (!pathYear) return;

        setYear(Number(pathYear));

    }, [pathYear]);

    useEffect(() => {
        if (!budgetYears) return;

        const newYears = budgetYears;

        next5Years.forEach(y => { if (!newYears.includes(y)) newYears.push(y); });

        setSelectableYears(newYears.sort());

    }, [budgetYears]);

    return (
        <BudgetPage title="Budget Report" breadcrumbs={[{ text: "Report", route: "/budget/report" }]}>
            <Section>
                <Row>
                    <Col>
                        <Form.Select value={year} onChange={e => yearChange(Number(e.currentTarget.value))}>
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
