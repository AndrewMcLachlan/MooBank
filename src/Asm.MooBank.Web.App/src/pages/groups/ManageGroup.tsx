
import { useParams } from "react-router";
import { useGroup } from "services";
import { GroupForm } from "./GroupForm";


export const ManageGroup = () => {

    const { id } = useParams<{ id: string }>();
    const { data: group } = useGroup(id!);

    return (
        <GroupForm group={group} />
    );
}
