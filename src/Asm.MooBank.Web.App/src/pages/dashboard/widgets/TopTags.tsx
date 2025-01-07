import { Widget } from "@andrewmclachlan/mooapp";
import { lastMonth } from "helpers/dateFns";
import { ReportType } from "models/reports";
import { TopTags } from "pages";
import { useAccounts } from "services";

export const TopTagsWidget: React.FC = () => {

    const {data: accounts, isLoading } = useAccounts();

    const account = accounts?.find(a => a.isPrimary === true) ?? accounts?.[0];

    return (
        <Widget title={(account && `Top Tags - ${account.name} - Last Month`) ?? 'Last Month'} size="double" titleSize={2} className="report" loading={isLoading}>
            {account && <TopTags accountId={account?.id} period={lastMonth} reportType={ReportType.Expenses} top={10} />}
        </Widget>
    );
};
