import { lastMonth } from "helpers/dateFns";
import { InOut } from "pages";
import { useAccounts } from "services";
import { Widget } from "./Widget";

export const InOutWidget: React.FC = () => {

    const {data: accounts, isLoading } = useAccounts();

    const id = accounts?.find(a => a.isPrimary === true)?.id ?? (accounts && accounts[0].id);

    return (
        <Widget title={(accounts && `${accounts[0].name} - Last Month`) ?? 'Last Month'} size={2} className="report inout" loading={isLoading}>
            {id && <InOut accountId={id} period={lastMonth} />}
        </Widget>
    );
};