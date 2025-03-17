import React from "react";

export const AccountBalance: React.FC<AccountBalanceProps> = (props) =>
    !props.balance ? null : <span className={`amount ${props.balance < 0 ? " negative" : ""}`}>{props.balance?.toLocaleString() + (props.balance < 0 ? "D" : "C") + "R"}</span>;


export interface AccountBalanceProps {
    balance: number;
}
