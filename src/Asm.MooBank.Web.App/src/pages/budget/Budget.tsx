import { Section, SectionTable } from "@andrewmclachlan/moo-ds";
import { format } from "date-fns/format";
import { isMonthSelected } from "helpers/dateFns";
import { useBudgetYear } from "hooks/useBudgetYear";
import * as Models from "models";
import { useEffect, useState } from "react";
import { Col, Form, Row } from "react-bootstrap";
import { useBudget, useBudgetYears } from "services/BudgetService";
import { BudgetPage } from "./BudgetPage";
import { BudgetTable } from "./BudgetTable";
import { MonthLine } from "./MonthLine";

export const Budget: React.FC = () => {

    const next5Years = [...Array(5).keys()].map(i => new Date().getFullYear() + i);

    const [year, setYear] = useBudgetYear();
    const [month, setMonth] = useState(-1);
    const [selectableYears, setSelectableYears] = useState(next5Years);
    const [filteredBudget, setFilteredBudget] = useState<Models.Budget>();

    const title = `Budget${year && ` - ${year}`}`;

    const { data: budget } = useBudget(year);

    const { data: budgetYears } = useBudgetYears();

    useEffect(() => {
        if (!budgetYears) return;

        const newYears = budgetYears;

        next5Years.forEach(y => { if (!newYears.includes(y)) newYears.push(y); });

        setSelectableYears(newYears);

    }, [budgetYears]);

    useEffect(() => {
        if (!budget || month === -1) {
            setFilteredBudget(budget);
            return;
        }

        setFilteredBudget({ ...budget, incomeLines: budget.incomeLines.filter(i => isMonthSelected(i.month, month)), expensesLines: budget.expensesLines.filter(e => isMonthSelected(e.month, month)) });
    }, [month, budget]);

    return (
        <BudgetPage title={title}>
            <Section>
                <Row>
                    <Col>
                        <Form.Select value={year} onChange={e => setYear(Number(e.currentTarget.value))}>
                            {selectableYears.map((y) =>
                                <option value={y} key={y}>{y}</option>
                            )}
                        </Form.Select>
                    </Col>
                    <Col>
                        <Form.Select value={month} onChange={e => setMonth(Number(e.currentTarget.value))}>
                            <option value={-1}>All Months</option>
                            {[...Array(12).keys()].map((y) =>
                                <option value={y} key={y}>{format(new Date(1, y), 'MMMM')}</option>
                            )}
                        </Form.Select>
                    </Col>
                </Row>
            </Section>
            <BudgetTable title="Income" year={year} lines={filteredBudget?.incomeLines} type="income" />
            <BudgetTable title="Expenses" year={year} lines={filteredBudget?.expensesLines} type="expenses" />

            {/*
            <Section className="budget" header="Income">
                <Table striped className="budget-list">
                    <thead>
                        <tr>
                            <th>Tag</th>
                            <th>Amount</th>
                            <th>When</th>
                            <th></th>
                        </tr>
                    </thead>
                    <tbody>
                        {filteredBudget?.incomeLines.map((b) =>
                            <BudgetLine year={year} budgetLine={b} key={b.id} />
                        )}
                        <NewBudgetLine year={year} income />
                    </tbody>
                    <tfoot>
                        <tr>
                            <td>Total</td>
                            <td colSpan={3}>{budget?.incomeLines.map(b => b.amount).reduce((total, current) => total + current, 0).toFixed(2)}</td>
                        </tr>
                    </tfoot>
                </Table>
            </Section>
            <Section className="budget" header="Expenses">
                <Table striped className="budget-list">
                    <thead>
                        <tr>
                            <th>Tag</th>
                            <th>Amount</th>
                            <th>When</th>
                            <th></th>
                        </tr>
                    </thead>
                    <tbody>
                        {filteredBudget?.expensesLines.map((b) =>
                            <BudgetLine year={year} budgetLine={b} key={b.id} />
                        )}

                        <NewBudgetLine year={year} />
                    </tbody>
                    <tfoot>
                        <tr>
                            <td>Total</td>
                            <td colSpan={3}>{budget?.expensesLines.map(b => b.amount).reduce((total, current) => total + current, 0).toFixed(2)}</td>
                        </tr>
                    </tfoot>
                </Table>
            </Section>
                        */}
            <SectionTable header="Monthly Budget" striped className="budget-list">
                <thead>
                    <tr>
                        <th className="column-15">Month</th>
                        <th className="column-15">Income</th>
                        <th className="column-15">Expenses</th>
                        <th className="column-15">Remainder</th>
                    </tr>
                </thead>
                <tbody>
                    {budget?.months.map((b) =>
                        <MonthLine month={b} key={b.month} />
                    )}
                </tbody>
            </SectionTable>
        </BudgetPage>
    );

}
