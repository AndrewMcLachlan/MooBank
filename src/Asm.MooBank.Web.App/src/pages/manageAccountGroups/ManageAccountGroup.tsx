
import { useParams } from "react-router-dom";
import { useAccountGroup } from "services";
import { AccountGroupForm } from "./AccountGroupForm";


export const ManageAccountGroup = () => {

    const { id } = useParams<{ id: string }>();
    const { data: accountGroup } = useAccountGroup(id!);

    return (
        <AccountGroupForm accountGroup={accountGroup} />
    );
}