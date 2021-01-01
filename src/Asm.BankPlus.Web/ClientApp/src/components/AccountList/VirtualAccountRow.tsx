import React, { Component } from "react";
import { connect } from "react-redux";

import { VirtualAccount } from "../../models";

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
                                <a href={"/transactions/transfer/" + this.props.account.id}>Transfer funds</a>
                            </li>
                            <li>
                                <a href={"/transactions/history/" + this.props.account.id}>Transactions</a>
                            </li>
                        </ul>
                    </nav>e
                </td>
                <td>{this.props.account.currentBalance.toFixed(2) + " " + (this.props.account.currentBalance < 0 ? "D" : "C") + "R"}</td>
            </tr>
        );
    }
}

export default connect()(VirtualAccountRow);

interface VirtualAccountRowProps {
    account: VirtualAccount;
}
