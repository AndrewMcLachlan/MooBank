import { emptyGroup } from "models";
import { AccountGroupForm } from "./AccountGroupForm";

export const CreateAccountGroup = () => (
    <AccountGroupForm accountGroup={emptyGroup} />
);
