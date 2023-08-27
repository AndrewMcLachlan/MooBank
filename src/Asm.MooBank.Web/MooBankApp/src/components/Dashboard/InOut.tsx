import { Section } from "@andrewmclachlan/mooapp";
import { last12Months, lastMonth, previousMonth } from "helpers/dateFns";
import { InOut } from "pages";
import { Spinner } from "react-bootstrap";
import { useAccounts } from "services";

export const InOutWidget: React.FC = () => {

    const {data: accounts, isLoading } = useAccounts();

    const id = accounts ? accounts[0].id : null;

    if (isLoading) return <Spinner />;

    return (
        <Section title={`${accounts[0].name} - Last Month`} size={3} className="inout">
            <InOut accountId={id} period={lastMonth} />
        </Section>
    )
};