import { useFormattedAccounts } from "services";
import { Widget } from "./Widget";
import { KeyValue } from "components/KeyValue";

export const PositionWidget: React.FC = () => {

    const { data: accounts, isLoading } = useFormattedAccounts();

    const positions = accounts?.accountGroups.filter(ag => ag.position);

    return (
        <Widget title="Summary" className="summary" loading={isLoading}>
            {positions?.map((ag, index) =>
                <KeyValue key={index}>
                    <div>{ag.name}</div>
                    <div className="amount">{ag.position.toLocaleString()}</div>
                </KeyValue>
            )}
        </Widget>
    );
};