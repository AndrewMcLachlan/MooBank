import { lastMonth } from "helpers/dateFns";
import { InOut } from "pages";
import { useAccounts, useInOutReport } from "services";
import { Widget } from "@andrewmclachlan/mooapp";

export const InOutWidget: React.FC = () => {

    const {data: accounts, isLoading } = useAccounts();

    const account = accounts?.find(a => a.isPrimary === true) ?? accounts?.[0];

    return (
        <Widget title={(account && `${account.name} - Last Month`) ?? 'Last Month'} size="single" titleSize={2} className="report inout" loading={isLoading}>
            {account && <InOut accountId={account?.id} period={lastMonth} useInOutReport={useInOutReport} />}
        </Widget>
    );
};
