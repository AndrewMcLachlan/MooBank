import { emptyGroup } from "models";
import { GroupForm } from "./GroupForm";

export const CreateGroup = () => (
    <GroupForm group={emptyGroup} />
);
