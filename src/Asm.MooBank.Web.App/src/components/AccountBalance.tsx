import React from "react";

export const AccountBalance: React.FC<AccountBalanceProps> = (props) => {

    return (<span className={`amount ${props.balance < 0 ? " negative" : ""}`}>{props.balance.toLocaleString() + (props.balance < 0 ? "D" : "C") + "R"}</span>);

}

 AccountBalance.displayName = "AccountBalanceProps";

 export interface AccountBalanceProps {
     balance: number;
 }