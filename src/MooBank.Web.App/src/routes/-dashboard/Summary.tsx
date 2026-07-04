import { useFormattedAccounts } from "hooks/useFormattedAccounts";
import { Section, Widget } from "@andrewmclachlan/moo-ds";
import { KeyValue } from "components/KeyValue";
import { Amount } from "components/Amount";
import { WidgetError } from "components/WidgetError";

export const SummaryWidget: React.FC = () => {

    const { data: accounts, isLoading, isError } = useFormattedAccounts();

    const totals = accounts?.groups.flatMap(g => g.instruments);
    const grandTotal = totals?.reduce((acc, i) => acc + i.currentBalanceLocalCurrency, 0);
    const groupTotals = accounts?.groups.filter(ag => ag.total);

    return (
        <Widget header="Summary" className="summary" loading={isLoading} size="single" to="/accounts">
            {isError ? <WidgetError /> : (
                <>
                    <KeyValue className="net-worth">
                        <div>Net Worth</div>
                        <Amount amount={grandTotal} positiveColour negativeColour plus minus />
                    </KeyValue>
                    <Section.Subheading>Groups</Section.Subheading>
                    {groupTotals?.map((ag, index) =>
                        <KeyValue key={index}>
                            <div>{ag.name}</div>
                            <div><Amount amount={ag.total} positiveColour negativeColour plus minus /></div>
                        </KeyValue>
                    )}
                </>
            )}
        </Widget>
    );
};
