import { Widget } from "@andrewmclachlan/mooapp";
import { lastMonth, lastMonthName } from "helpers/dateFns";
import { Breakdown } from "pages";
import { useAccounts } from "services";

export const BreakdownWidget: React.FC = () => {

    const {data: accounts, isLoading } = useAccounts();

    const account = accounts?.find(a => a.isPrimary === true) ?? accounts?.[0];

    return (
        <Widget header={(account && `Breakdown - ${account.name} - ${lastMonthName}`) ?? lastMonthName} size="double" headerSize={2} className="report" loading={isLoading}>
            {account && <Breakdown accountId={account?.id} period={lastMonth} reportType={"Debit"} />}
        </Widget>
    );
};
