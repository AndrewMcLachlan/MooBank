import { DeleteIcon, Icon } from "@andrewmclachlan/moo-ds";
import { useAccount } from "components";
import { formatDate } from "helpers/dateFns";
import type { InstitutionAccount, LogicalAccount } from "api/types.gen";
import React from "react";
import { useCloseInstitutionAccount, useImporterTypes, useInstitutions, useInstitutionsByAccountType } from "services";

export const InstitutionAccountRow: React.FC<InstitutionAccountRowProps> = ({ institutionAccount, onClick }) => {

    const account = useAccount() as LogicalAccount;
    const { data: institutions } = useInstitutionsByAccountType(account?.accountType);
    const { data: importerTypes } = useImporterTypes();

    const closeAccount = useCloseInstitutionAccount();

    return (
        <tr key={institutionAccount.id}>
            <td>{institutions?.find(i => i.id === institutionAccount.institutionId)?.name}</td>
            <td>{institutionAccount.name}</td>
            <td>{formatDate(institutionAccount.openedDate)}</td>
            <td>{formatDate(institutionAccount.closedDate)}</td>
            {account?.controller === "Import" && <td>{importerTypes?.find(i => i.id === institutionAccount.importerTypeId)?.name}</td>}
            <td className="row-action">
                <div hidden={institutionAccount.closedDate !== null}>
                    <Icon icon="pen-to-square" title="Edit Details" onClick={() => onClick(institutionAccount)} />
                    <Icon icon="door-closed" title="Close Account" onClick={() => { closeAccount.mutateAsync(account.id, institutionAccount.id); }} />
                </div>
            </td>
        </tr>
    );
};

export interface InstitutionAccountRowProps {
    institutionAccount: InstitutionAccount;
    onClick: (institutionAccount: InstitutionAccount) => void;
}