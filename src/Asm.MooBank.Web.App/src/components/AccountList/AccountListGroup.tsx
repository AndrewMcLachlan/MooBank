import React from "react";
import { Spinner } from "@andrewmclachlan/moo-ds";
import { useNavigate } from "react-router";

import { Icon, SectionTable } from "@andrewmclachlan/moo-ds";
import { Reports } from "@andrewmclachlan/moo-icons";
import { AccountListGroup as Model } from "models";
import { AccountRow } from "./AccountRow";
import { Amount } from "components/Amount";

export interface AccountListGroupProps {
    group: Model;
    isLoading: boolean;
}

export const AccountListGroup: React.FC<AccountListGroupProps> = ({ group, isLoading }) => {
    const navigate = useNavigate();
    
    const handleReportClick = () => {
        if (group.id) {
            navigate(`/groups/${group.id}/reports/monthly-balances`);
        }
    };

    const headerContent = (
        <header>
            <h3>{group?.name}</h3>
            {group.id && (
                <Icon 
                    icon={Reports}
                    className="clickable" 
                    title="View Monthly Balances Report" 
                    onClick={handleReportClick}
                />
            )}
            
        </header>
    );

    return (
        <SectionTable className="accounts" hover header={headerContent} headerSize={2} hidden={group.instruments.length === 0}>
            <thead>
                <tr>
                    <th className="expander d-none d-sm-table-cell"></th>
                    <th>Name</th>
                    <th className="d-none d-sm-table-cell">Type</th>
                    <th className="number">Balance</th>
                </tr>
            </thead>
            <tbody>
                {!isLoading && group && group.instruments.map(a => <AccountRow key={a.id} instrument={a} />)}
                {isLoading &&
                    <tr><td colSpan={4} className="spinner">
                        <Spinner animation="border" />
                    </td></tr>}
            </tbody>
            {group.total !== undefined && group.total !== null && <tfoot>
                <tr className="total">
                    <td className="d-none d-sm-table-cell" />
                    <td>Total</td>
                    <td className="d-none d-sm-table-cell"></td>
                    <td className="number"><Amount amount={group?.total} negativeColour minus /></td>
                </tr>
            </tfoot>}
        </SectionTable>
    );
};
