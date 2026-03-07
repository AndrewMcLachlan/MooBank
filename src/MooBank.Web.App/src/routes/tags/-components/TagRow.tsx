import React, { useState } from "react";

import type { Tag } from "api/types.gen";
import { ClickableIcon, EditColumn, Icon, useUpdatingState } from "@andrewmclachlan/moo-ds";
import { useDeleteTag } from "../-hooks/useDeleteTag";
import { useUpdateTag } from "../-hooks/useUpdateTag";
import { TransactionTagTransactionTagPanel } from "./TagTagPanel";
import { TransactionTagDetails } from "./TagDetails";
import { DeleteIcon } from "@andrewmclachlan/moo-ds";


export const TransactionTagRow: React.FC<TransactionTagRowProps> = (props) => {

    const [showDetails, setShowDetails] = useState(false);

    const { tag, ...tagRow } = useTagRowEvents(props);

    if (!tag) return (
        <tr className="empty-row"><td colSpan={3}>&nbsp;</td></tr>
    );

    return (
        <>
            <TransactionTagDetails tag={tag} show={showDetails} onHide={() => setShowDetails(false)} />
            <tr>
                <EditColumn value={tag.name} onChange={t => tagRow.updateTag(t.value)} />
                <TransactionTagTransactionTagPanel as="td" tag={tag} />
                <td className="row-action">
                    <span onClick={() => setShowDetails(true)}><Icon className="clickable" icon="pen-to-square" title="Edit Details" /></span>
                    <span onClick={tagRow.deleteTag}><DeleteIcon /></span>
                </td>
            </tr>
        </>
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
}
