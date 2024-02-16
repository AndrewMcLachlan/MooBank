import { emptyAccountGroup } from "models";
import { AccountGroupForm } from "./AccountGroupForm";

export const CreateAccountGroup = () => (
    <AccountGroupForm accountGroup={emptyAccountGroup} />
);