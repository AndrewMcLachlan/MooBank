import { Section } from "@andrewmclachlan/mooapp";
import { lastMonth } from "helpers/dateFns";
import { InOut } from "pages";
import { Spinner } from "react-bootstrap";
import { useAccounts } from "services";

export const InOutWidget: React.FC = () => {

    const {data: accounts, isLoading } = useAccounts();

    if (!accounts?.length) return null;

    const id = accounts.find(a => a.isPrimary === true)?.id ?? accounts[0].id;

    if (isLoading) return <Spinner />;

    return (
        <Section title={`${accounts[0].name} - Last Month`} size={3} className="inout">
            <InOut accountId={id} period={lastMonth} />
        </Section>
    )
};