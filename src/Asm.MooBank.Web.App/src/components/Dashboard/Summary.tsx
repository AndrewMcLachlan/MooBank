import { useFormattedAccounts } from "services";
import { Section, Widget } from "@andrewmclachlan/mooapp";
import { KeyValue } from "components/KeyValue";
import { Amount } from "components/Amount";

export const SummaryWidget: React.FC = () => {

    const { data: accounts, isLoading } = useFormattedAccounts();

    const totals = accounts?.accountGroups.filter(ag => ag.position);
    const grandTotal = totals?.reduce((acc, ag) => acc + ag.position, 0);

    return (
        <Widget title="Summary" className="summary" loading={isLoading}>
            <KeyValue className="net-worth">
                <div>Net Worth</div>
                <Amount amount={grandTotal} colour plusminus />
            </KeyValue>
            <Section.SubHeading>Groups</Section.SubHeading>
            {totals?.map((ag, index) =>
                <KeyValue key={index}>
                    <div>{ag.name}</div>
                    <div className="amount">{ag.position.toLocaleString()}</div>
                </KeyValue>
            )}
        </Widget>
    );
};
