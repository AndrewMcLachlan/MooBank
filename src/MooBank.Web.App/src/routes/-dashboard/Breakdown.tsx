import { Widget } from "@andrewmclachlan/moo-ds";
import { lastMonth, lastMonthName } from "utils/dateFns";
import { Breakdown } from "../accounts/$id/reports/-components/Breakdown";
import { useAccounts } from "hooks/useAccounts";
import { WidgetError } from "components/WidgetError";

export const BreakdownWidget: React.FC = () => {

    const {data: accounts, isLoading, isError } = useAccounts();

    const account = accounts?.find(a => a.isPrimary === true) ?? accounts?.[0];

    return (
        <Widget header={(account && `Breakdown - ${account.name} - ${lastMonthName}`) ?? lastMonthName} size="double" headerSize={2} className="report" loading={isLoading} to={account ? `/accounts/${account.id}/reports/breakdown?period=1` : undefined}>
            {isError ? <WidgetError /> : account && <Breakdown accountId={account?.id} period={lastMonth} reportType={"Debit"} />}
        </Widget>
    );
};
