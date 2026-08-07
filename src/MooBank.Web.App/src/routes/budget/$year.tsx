import { createFileRoute, redirect, useNavigate } from "@tanstack/react-router";
import { Col, IconButton, Row, SectionTable, Stack, Tab, Tabs } from "@andrewmclachlan/moo-ds";
import { Sparkle } from "@andrewmclachlan/moo-icons";
import { useBudget } from "./-hooks/useBudget";
import { useBudgetYears } from "./-hooks/useBudgetYears";
import { useGenerateBudget } from "./-hooks/useGenerateBudget";
import { BudgetPage } from "./-components/BudgetPage";
import { BudgetSummary } from "./-components/BudgetSummary";
import { BudgetTable } from "./-components/BudgetTable";
import { BudgetSurplusChart } from "./-components/BudgetSurplusChart";
import { BudgetCumulativeChart } from "./-components/BudgetCumulativeChart";
import { BudgetYearPicker } from "./-components/BudgetYearPicker";
import { MonthLine } from "./-components/MonthLine";
import { isBudgetYear, currentBudgetYear } from "./-utils/budgetYear";

export const Route = createFileRoute("/budget/$year")({
    beforeLoad: ({ params }) => {
        // A hand-typed or stale URL must not reach the API as NaN.
        if (!isBudgetYear(params.year)) {
            throw redirect({ to: "/budget/$year", params: { year: String(currentBudgetYear()) } } as any);
        }
    },
    component: Budget,
});

function Budget() {

    const { year: yearParam } = Route.useParams();
    const year = Number(yearParam);

    const navigate = useNavigate();
    const setYear = (newYear: number) => navigate({ to: "/budget/$year", params: { year: String(newYear) } } as any);

    const title = `Budget - ${year}`;

    const { data: budget } = useBudget(year);
    const { data: budgetYears } = useBudgetYears();
    const { generate, isPending: isGenerating } = useGenerateBudget();

    const onGenerate = () => {
        const hasLines = (budget?.incomeLines.length ?? 0) + (budget?.expensesLines.length ?? 0) > 0;
        if (hasLines && !window.confirm("Adds budget lines for tags you haven't budgeted yet. Existing lines are left untouched. Continue?")) return;
        generate(year);
    };

    return (
        <BudgetPage title={title} actions={[
            <BudgetYearPicker key="year" year={year} years={budgetYears} onChange={setYear} />,
            <IconButton key="generate" badge variant="primary" icon={Sparkle} onClick={onGenerate} disabled={isGenerating} title="Build budget lines from your transaction history">
                {isGenerating ? "Generating…" : "Generate from history"}
            </IconButton>
        ]}>
            <Tabs defaultActiveKey="summary">
                <Tab eventKey="summary" title="Summary">
                    <Stack>
                    <BudgetSummary budget={budget} />
                    <Row>
                        <Col xxl={6} xl={12}>
                            <BudgetSurplusChart months={budget?.months} />
                        </Col>
                        <Col xxl={6} xl={12}>
                            <BudgetCumulativeChart months={budget?.months} />
                        </Col>
                    </Row>
                    <SectionTable header="Monthly Budget" striped className="budget-list" loading={!budget} loadingRows={12}>
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
                    </Stack>
                </Tab>
                <Tab eventKey="income" title="Income">
                    <BudgetTable year={year} lines={budget?.incomeLines} type="Income" />
                </Tab>
                <Tab eventKey="expenses" title="Expenses">
                    <BudgetTable year={year} lines={budget?.expensesLines} type="Expenses" />
                </Tab>
            </Tabs>
        </BudgetPage>
    );
}
