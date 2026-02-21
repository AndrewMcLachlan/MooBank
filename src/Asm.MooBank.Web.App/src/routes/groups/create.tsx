import { createFileRoute } from "@tanstack/react-router";
import { emptyGroup } from "helpers/groups";
import { GroupForm } from "./-components/GroupForm";

export const Route = createFileRoute("/groups/create")({
    component: CreateGroup,
});

function CreateGroup() {
    return (
        <GroupForm group={emptyGroup} />
    );
}
