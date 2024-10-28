import { Widget } from "@andrewmclachlan/mooapp";
import { lastMonth } from "helpers/dateFns";
import { ReportType } from "models/reports";
import { Breakdown } from "pages";
import { useAccounts } from "services";

export const BreakdownWidget: React.FC = () => {

    const {data: accounts, isLoading } = useAccounts();

    const account = accounts?.find(a => a.isPrimary === true) ?? accounts?.[0];

    return (
        <Widget title={(account && `Breakdown - ${account.name} - Last Month`) ?? 'Last Month'} size="double" titleSize={2} className="report" loading={isLoading}>
            {account && <Breakdown accountId={account?.id} period={lastMonth} reportType={ReportType.Expenses} />}
        </Widget>
    );
};
