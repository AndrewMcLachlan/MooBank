import { useIdParams, usePageTitle } from "hooks"
import { Page } from "layouts";
import { useAccount } from "services"
import { useBudget, useDeleteBudgetLine } from "services/BudgetService";
import { Table } from "react-bootstrap";
import { BudgetLine } from "./BudgetLine";
import { NewBudgetLine } from "./NewBudgetLine";

export const Budget: React.FC = () => {

    usePageTitle("Budget");

    const id = useIdParams();
    const account = useAccount(id);
    const budget = useBudget(id);

    return (
        <Page title="Budget">
            <Page.Header title="Budget" breadcrumbs={[[account.data?.name, `/accounts/${id}`], ["Budget", `/accounts/${id}/budget`]]} />
            <Page.Content>
                <section className="budget">
                    <h2>Income</h2>
                    <Table striped className="budget-list">
                        <tbody>
                            {budget.data?.map((b) =>
                                <BudgetLine budgetLine={b} key={b.id} />
                            )}
                            <NewBudgetLine income accountId={id} />
                        </tbody>
                    </Table>
                </section>
                <section className="budget">
                    <h2>Expenses</h2>
                    <Table striped className="budget-list">
                        <tbody>
                            {budget.data?.map((b) =>
                                <BudgetLine budgetLine={b} key={b.id} />
                            )}

                            <NewBudgetLine accountId={id} />
                        </tbody>
                    </Table>
                </section>
            </Page.Content>
        </Page>
    );

}