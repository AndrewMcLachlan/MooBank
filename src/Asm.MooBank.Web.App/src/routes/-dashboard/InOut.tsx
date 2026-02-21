import { lastMonth, lastMonthName } from "helpers/dateFns";
import { InOut } from "../accounts/$id/reports/-components/InOut";
import { useAccounts, useInOutReport } from "services";
import { Widget } from "@andrewmclachlan/moo-ds";

export const InOutWidget: React.FC = () => {

    const { data: accounts, isLoading } = useAccounts();

    const account = accounts?.find(a => a.isPrimary === true) ?? accounts?.[0];

    return (
        <Widget header={(account && `${account.name} - ${lastMonthName}`) ?? lastMonthName} size="single" headerSize={2} className="report inout" loading={isLoading}>
            {account && <InOut accountId={account?.id} period={lastMonth} useInOutReport={useInOutReport} />}
        </Widget>
    );
};
