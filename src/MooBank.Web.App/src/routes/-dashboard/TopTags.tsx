import { Widget } from "@andrewmclachlan/moo-ds";
import { lastMonth, lastMonthName } from "utils/dateFns";
import { TopTags } from "../accounts/$id/reports/-components/TopTags";
import { useAccounts } from "hooks/useAccounts";

export const TopTagsWidget: React.FC = () => {

    const {data: accounts, isLoading } = useAccounts();

    const account = accounts?.find(a => a.isPrimary === true) ?? accounts?.[0];

    return (
        <Widget header={(account && `Top Tags - ${account.name} - ${lastMonthName}`) ?? lastMonthName} size="double" headerSize={2} className="report" loading={isLoading}>
            {account && <TopTags accountId={account?.id} period={lastMonth} reportType={"Debit"} top={10} />}
        </Widget>
    );
};
