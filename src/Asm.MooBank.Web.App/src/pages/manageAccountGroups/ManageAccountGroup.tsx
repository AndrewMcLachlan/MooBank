import React, { useState } from "react";

import { useAccountGroup } from "services";
import { AccountGroupForm } from "./AccountGroupForm";
import { useParams } from "react-router-dom";


export const ManageAccountGroup = () => {

    const { id } = useParams<{ id: string }>();
    const { data: accountGroup } = useAccountGroup(id!);

    return (
        <AccountGroupForm accountGroup={accountGroup} />
    );
}