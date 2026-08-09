import { createFileRoute } from "@tanstack/react-router";
import React from "react";

import { Page } from "@andrewmclachlan/moo-app";
import { IconButton, LoadingTableRows, SectionTable } from "@andrewmclachlan/moo-ds";
import { useGroups } from "./-hooks/useGroups";
import { useReorderGroups } from "./-hooks/useReorderGroups";

import { useNavigate } from "@tanstack/react-router";
import { GroupRow } from "./-components/GroupRow";

import {
    DndContext,
    KeyboardSensor,
    PointerSensor,
    closestCenter,
    useSensor,
    useSensors,
    type DragEndEvent,
} from "@dnd-kit/core";
import {
    SortableContext,
    arrayMove,
    sortableKeyboardCoordinates,
    verticalListSortingStrategy,
} from "@dnd-kit/sortable";

export const Route = createFileRoute("/groups/")({
    component: ManageGroups,
});

function ManageGroups() {

    const navigate = useNavigate();

    const { data } = useGroups();
    const { reorder } = useReorderGroups();

    const sensors = useSensors(
        // A row is also a link to the group, so a press only becomes a drag once it has travelled
        // far enough to have been meant as one.
        useSensor(PointerSensor, { activationConstraint: { distance: 8 } }),
        useSensor(KeyboardSensor, { coordinateGetter: sortableKeyboardCoordinates }),
    );

    const onDragEnd = ({ active, over }: DragEndEvent) => {
        if (!over || active.id === over.id || !data) return;

        const from = data.findIndex(g => g.id === active.id);
        const to = data.findIndex(g => g.id === over.id);

        if (from === -1 || to === -1) return;

        // The command wants every group, in order, so the whole list goes back rather than the one
        // that moved.
        reorder(arrayMove(data, from, to).map(g => g.id));
    };

    const groupRows: React.ReactNode[] = data?.map(a => <GroupRow key={a.id} group={a} />) ?? [<LoadingTableRows key={1} rows={5} cols={4} />];

    return (
        <Page title="Groups" breadcrumbs={[{ text: "Groups", route: "/groups" }]} actions={[<IconButton badge key="add" onClick={() => navigate({ to: "/groups/create" })} icon="plus">Create Group</IconButton>]}>
            <DndContext sensors={sensors} collisionDetection={closestCenter} onDragEnd={onDragEnd}>
                <SectionTable hover striped className="group-list">
                    <thead>
                        <tr>
                            <th className="drag-handle"><span className="visually-hidden">Reorder</span></th>
                            <th className="column-25">Name</th>
                            <th>Description</th>
                            <th className="column-10 row-action">Show Total</th>
                        </tr>
                    </thead>
                    <tbody>
                        <SortableContext items={data?.map(g => g.id) ?? []} strategy={verticalListSortingStrategy}>
                            {groupRows}
                        </SortableContext>
                    </tbody>
                </SectionTable>
            </DndContext>
        </Page>
    );
}
