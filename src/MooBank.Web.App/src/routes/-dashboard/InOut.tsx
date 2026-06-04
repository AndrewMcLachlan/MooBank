import { lastMonth, lastMonthName } from "utils/dateFns";
import { InOut } from "../accounts/$id/reports/-components/InOut";
import { useAccounts } from "hooks/useAccounts";
import { useInOutReport } from "hooks/useInOutReport";
import { Widget } from "@andrewmclachlan/moo-ds";
import { WidgetError } from "components/WidgetError";

export const InOutWidget: React.FC = () => {

    const { data: accounts, isLoading, isError } = useAccounts();

    const account = accounts?.find(a => a.isPrimary === true) ?? accounts?.[0];

    return (
        <Widget header={(account && `${account.name} - ${lastMonthName}`) ?? lastMonthName} size="single" headerSize={2} className="report inout" loading={isLoading}>
            {isError ? <WidgetError /> : account && <InOut accountId={account?.id} period={lastMonth} useInOutReport={useInOutReport} />}
        </Widget>
    );
};
