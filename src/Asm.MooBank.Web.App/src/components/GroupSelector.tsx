import {  FormComboBox } from "@andrewmclachlan/moo-ds";
import React from "react";
import { useGroups } from "services";

export const GroupSelector: React.FC = () => {

    const { data: groups } = useGroups();

    return (
        <FormComboBox placeholder="Select a group..." items={groups ?? []} labelField={i => i.name} valueField={i => i.id} />
    );
}

GroupSelector.displayName = "GroupSelector";
