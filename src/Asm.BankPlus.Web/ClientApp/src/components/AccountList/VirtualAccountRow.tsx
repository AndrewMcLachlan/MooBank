import React, { Component } from "react";
import { connect } from "react-redux";

import { VirtualAccount } from "../../store/state";

class VirtualAccountRow extends Component<VirtualAccountRowProps, any> {

    public render() {
        return (
            <tr>
                <td className="account">
                    <div className="name">{this.props.account.name}</div>
                    <div className="description">{this.props.account.description}</div>
                    <nav className="desktop">
                        <ul>
                            <li>
                                <a href={"/transactions/transfer/" + this.props.account.virtualAccountId}>Transfer funds</a>
                            </li>
                            <li>
                                <a href={"/transactions/history/" + this.props.account.virtualAccountId}>Transactions</a>
                            </li>
                        </ul>
                    </nav>e
                </td>
                <td>{this.props.account.balance.toFixed(2) + " " + (this.props.account.balance < 0 ? "D" : "C") + "R"}</td>
            </tr>
        );
    }
}

export default connect()(VirtualAccountRow);

interface VirtualAccountRowProps {
    account: VirtualAccount;
}
