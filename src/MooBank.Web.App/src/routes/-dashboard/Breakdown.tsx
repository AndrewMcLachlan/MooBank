import { Widget } from "@andrewmclachlan/moo-ds";
import { useNavigate } from "@tanstack/react-router";
import { lastMonth, lastMonthName } from "utils/dateFns";
import { Breakdown } from "../accounts/$id/reports/-components/Breakdown";
import { useAccounts } from "hooks/useAccounts";
import { WidgetError } from "components/WidgetError";
import type { TagValue } from "api/types.gen";

const reportType = "Debit" as const;

export const BreakdownWidget: React.FC = () => {

    const {data: accounts, isLoading, isError } = useAccounts();
    const navigate = useNavigate();

    const account = accounts?.find(a => a.isPrimary === true) ?? accounts?.[0];

    const selectedTagChanged = (clickedTag: TagValue) => {
        // period=1 (Last Month) scopes the target page to the dashboard's period.
        if (!clickedTag.hasChildren) {
            const url = !clickedTag.tagId
                ? `/accounts/${account!.id}?untagged=true&period=1`
                : `/accounts/${account!.id}?tag=${clickedTag.tagId}&type=${reportType}&period=1`;
            navigate({ to: url });
            return;
        }

        navigate({ to: `/accounts/${account!.id}/reports/breakdown/${clickedTag.tagId}?period=1` });
    };

    return (
        <Widget header={(account && `Breakdown - ${account.name} - ${lastMonthName}`) ?? lastMonthName} size="double" headerSize={2} className="report" loading={isLoading} to={account ? `/accounts/${account.id}/reports/breakdown?period=1` : undefined}>
            {isError ? <WidgetError /> : account && <Breakdown accountId={account.id} period={lastMonth} reportType={reportType} selectedTagChanged={selectedTagChanged} />}
        </Widget>
    );
};
