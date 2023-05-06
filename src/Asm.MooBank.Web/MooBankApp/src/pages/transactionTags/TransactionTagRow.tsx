import React, { useState, useEffect } from "react";

import { TransactionTag } from "models";
import { ClickableIcon, EditColumn } from "@andrewmclachlan/mooapp";
import { useDeleteTag, useTags, useUpdateTag } from "services";
import { TransactionTagTransactionTagPanel } from "./TransactionTagTransactionTagPanel";
import { TransactionTagDetails } from "./TransactionTagDetails";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { useUpdatingState } from "hooks";

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
                <EditColumn value={tag.name} onChange={(tagRow.updateTag)} />
                <TransactionTagTransactionTagPanel as="td" tag={tag} />
                <td className="row-action">
                    <span onClick={() => setShowDetails(true)}><ClickableIcon icon="pen-to-square" title="Edit Details" size="1x" /></span>
                    <span onClick={tagRow.deleteTag}><ClickableIcon icon="trash-alt" title="Delete" size="1x" /></span>
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
            deleteTransactionTag.mutate({ id: props.tag.id });
        }
    }

    const updateTag = (name: string) => {
        updateTransactionTag.mutate({ ...tag, name });
        setTag({ ...tag, name });
    }

    return {
        deleteTag,
        updateTag,
        tag,
    };
}

export interface TransactionTagRowProps {
    tag?: TransactionTag;
}