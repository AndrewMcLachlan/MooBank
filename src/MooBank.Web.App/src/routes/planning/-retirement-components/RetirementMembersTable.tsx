import { Badge, SectionTable } from "@andrewmclachlan/moo-ds";
import type { RetirementMemberOutcome } from "api/types.gen";
import { Amount } from "components";

interface RetirementMembersTableProps {
    members: RetirementMemberOutcome[];
    currencyCode: string;
}

export const RetirementMembersTable: React.FC<RetirementMembersTableProps> = ({ members, currencyCode }) => {

    if (members.length === 0) return null;

    return (
        <SectionTable striped hover header="By Person">
            <thead>
                <tr>
                    <th>Name</th>
                    <th>Age</th>
                    <th>Retires</th>
                    <th>Balance Today</th>
                    <th>At Retirement</th>
                    <th>In Today's Dollars</th>
                </tr>
            </thead>
            <tbody>
                {members.map(member => (
                    <tr key={member.memberId}>
                        <td>{member.name}</td>
                        <td>{member.currentAge}</td>
                        <td>
                            {member.alreadyRetired
                                ? <Badge pill muted bg="success">Retired</Badge>
                                : <>{member.retirementYear} <span className="retirement-age-note">at {member.retirementAge}</span></>}
                        </td>
                        <td><Amount amount={member.currentBalance} currencyCode={currencyCode} decimalPlaces={0} /></td>
                        <td><Amount amount={member.balanceAtRetirement} currencyCode={currencyCode} decimalPlaces={0} /></td>
                        <td><Amount amount={member.balanceAtRetirementInTodaysDollars} currencyCode={currencyCode} decimalPlaces={0} /></td>
                    </tr>
                ))}
            </tbody>
        </SectionTable>
    );
};
