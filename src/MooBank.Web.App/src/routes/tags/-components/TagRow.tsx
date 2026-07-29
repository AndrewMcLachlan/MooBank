import React from "react";

import type { Tag } from "api/types.gen";
import { EditColumn, Icon, LoadingTableRow, useUpdatingState } from "@andrewmclachlan/moo-ds";
import { useDeleteTag } from "../-hooks/useDeleteTag";
import { useUpdateTag } from "../-hooks/useUpdateTag";
import { TransactionTagTransactionTagPanel } from "./TagTagPanel";
import { DeleteIcon } from "@andrewmclachlan/moo-ds";
import { colourRowProps } from "components";


export const TransactionTagRow: React.FC<TransactionTagRowProps> = (props) => {

    const { tag, ...tagRow } = useTagRowEvents(props);

    if (!tag) return <LoadingTableRow cols={3} />;

    return (
        <tr {...colourRowProps(tag.colour)}>
            <EditColumn value={tag.name} onChange={t => tagRow.updateTag(t.value)}>
                {tag.name}
            </EditColumn>
            <TransactionTagTransactionTagPanel as="td" tag={tag} />
            <td className="row-action">
                <span onClick={() => props.onEdit?.(tag)}><Icon className="clickable" icon="pen-to-square" title="Edit Details" /></span>
                <span onClick={tagRow.deleteTag}><DeleteIcon /></span>
            </td>
        </tr>
    );
}

function useTagRowEvents(props: TransactionTagRowProps) {

    const [tag, setTag] = useUpdatingState(props.tag);

    const updateTransactionTag = useUpdateTag();
    const deleteTransactionTag = useDeleteTag();

    const deleteTag = () => {

        if (window.confirm("Deleting this tag will remove it from all rules and transactions. Are you sure?")) {
            deleteTransactionTag.mutate({ path: { id: props.tag.id } });
        }
    }

    const updateTag = (name: string) => {
        updateTransactionTag.mutate({ ...tag!, name });
        setTag({ ...tag!, name });
    }

    return {
        deleteTag,
        updateTag,
        tag,
    };
}

export interface TransactionTagRowProps {
    tag?: Tag;
    onEdit?: (tag: Tag) => void;
}
