import { useIdParams } from "@andrewmclachlan/mooapp";
import { usePageTitle } from "hooks";
import { Page } from "layouts";
import { useAccount } from "services"
import { useBudget, useBudgetYears, useDeleteBudgetLine } from "services/BudgetService";
import { Col, Form, Row, Table } from "react-bootstrap";
import { BudgetLine } from "./BudgetLine";
import { NewBudgetLine } from "./NewBudgetLine";
import { MonthLine } from "./MonthLine";
import { useEffect, useState } from "react";
import Select, { GroupBase, OptionsOrGroups } from "react-select";
import format from "date-fns/format";
import * as Models from "models";
import { isMonthSelected } from "helpers/dateFns";

export const Budget: React.FC = () => {

    const next5Years = [...Array(5).keys()].map(i => new Date().getFullYear() + i);

    const [year, setYear] = useState(new Date().getFullYear());
    const [month, setMonth] = useState(-1);
    const [selectableYears, setSelectableYears] = useState(next5Years);
    const [filteredBudget, setFilteredBudget] = useState<Models.Budget>();

    const title = `Budget${year && ` - ${year}`}`;

    usePageTitle(title);

    const id = useIdParams();
    const account = useAccount(id);
    const { data: budget } = useBudget(id, year);

    const { data: budgetYears } = useBudgetYears(id);

    useEffect(() => {
        if (!budgetYears) return;

        const newYears = budgetYears;

        next5Years.forEach(y => { if (!newYears.includes(y)) newYears.push(y); });

        setSelectableYears(newYears);

    }, [budgetYears]);

    useEffect(()=> {
        if (!budget || month === -1) {
            setFilteredBudget(budget);
            return;
        }

        setFilteredBudget({ ...budget, incomeLines: budget.incomeLines.filter(i => isMonthSelected(i.month, month)), expensesLines: budget.expensesLines.filter(e => isMonthSelected(e.month, month))});
    }, [month, budget]);

    return (
        <Page title="Budget">
            <Page.Header title={title} breadcrumbs={[[account.data?.name, `/accounts/${id}`], ["Budget", `/accounts/${id}/budget`]]} />
            <Page.Content>
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
                            <option value={y} key={y}>{format(new Date(1,y), 'MMMM')}</option>
                        )}
                    </Form.Select>
                    </Col>
                </Row>
                <section className="budget">
                    <h2>Income</h2>
                    <Table striped className="budget-list">
                        <tbody>
                            {filteredBudget?.incomeLines.map((b) =>
                                <BudgetLine accountId={id} year={year} budgetLine={b} key={b.id} />
                            )}
                            <NewBudgetLine year={year} income accountId={id} />
                        </tbody>
                        <tfoot>
                            <tr>
                                <td>Total</td>
                                <td colSpan={3}>{budget?.incomeLines.map(b => b.amount).reduce((total, current) => total + current, 0).toFixed(2)}</td>
                            </tr>
                        </tfoot>
                    </Table>
                </section>
                <section className="budget">
                    <h2>Expenses</h2>
                    <Table striped className="budget-list">
                        <tbody>
                            {filteredBudget?.expensesLines.map((b) =>
                                <BudgetLine accountId={id} year={year} budgetLine={b} key={b.id} />
                            )}

                            <NewBudgetLine year={year} accountId={id} />
                        </tbody>
                        <tfoot>
                            <tr>
                                <td>Total</td>
                                <td colSpan={3}>{budget?.expensesLines.map(b => b.amount).reduce((total, current) => total + current, 0).toFixed(2)}</td>
                            </tr>
                        </tfoot>
                    </Table>
                </section>
                <section>
                    <h2>Monthly Budget</h2>
                    <Table striped className="budget-list">
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
                    </Table>
                </section>
            </Page.Content>
        </Page>
    );

}