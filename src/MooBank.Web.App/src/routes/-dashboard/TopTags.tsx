import { Widget } from "@andrewmclachlan/moo-ds";
import { lastMonth, lastMonthName } from "utils/dateFns";
import { TopTags } from "../accounts/$id/reports/-components/TopTags";
import { useAccounts } from "hooks/useAccounts";
import { WidgetError } from "components/WidgetError";

export const TopTagsWidget: React.FC = () => {

    const {data: accounts, isLoading, isError } = useAccounts();

    const account = accounts?.find(a => a.isPrimary === true) ?? accounts?.[0];

    return (
        <Widget header={(account && `Top Tags - ${account.name} - ${lastMonthName}`) ?? lastMonthName} size="double" headerSize={2} className="report" loading={isLoading} to={account ? `/accounts/${account.id}/reports/all-tag-average?period=1` : undefined}>
            {isError ? <WidgetError /> : account && <TopTags accountId={account?.id} period={lastMonth} reportType={"Debit"} top={10} periodId="1" />}
        </Widget>
    );
};
