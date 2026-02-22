import { createFileRoute } from "@tanstack/react-router";
import { useParams } from "@tanstack/react-router";
import { useGroup } from "../-hooks/useGroup";
import { GroupForm } from "../-components/GroupForm";

export const Route = createFileRoute("/groups/$id/manage")({
    component: ManageGroup,
});

function ManageGroup() {

    const { id } = useParams({ strict: false });
    const { data: group } = useGroup(id!);

    return (
        <GroupForm key={group?.id} group={group} />
    );
}
