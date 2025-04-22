import { useFormattedAccounts } from "services";
import { Section, Widget } from "@andrewmclachlan/mooapp";
import { KeyValue } from "components/KeyValue";
import { Amount } from "components/Amount";

export const SummaryWidget: React.FC = () => {

    const { data: accounts, isLoading } = useFormattedAccounts();

    const totals = accounts?.groups.flatMap(g => g.instruments);
    const grandTotal = totals?.reduce((acc, i) => acc + i.currentBalanceLocalCurrency, 0);
    const groupTotals = accounts?.groups.filter(ag => ag.total);

    return (
        <Widget header="Summary" className="summary" loading={isLoading} size="single">
            <KeyValue className="net-worth">
                <div>Net Worth</div>
                <Amount amount={grandTotal} colour plusminus />
            </KeyValue>
            <Section.Subheading>Groups</Section.Subheading>
            {groupTotals?.map((ag, index) =>
                <KeyValue key={index}>
                    <div>{ag.name}</div>
                    <div><Amount amount={ag.total} colour plusminus /></div>
                </KeyValue>
            )}
        </Widget>
    );
};
