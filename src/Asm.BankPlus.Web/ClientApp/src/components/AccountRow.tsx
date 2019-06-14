import React, { Component } from "react";
import { connect } from "react-redux";

class AccountRow extends Component<any, any> {
    constructor(props) {
        super(props);
    }

    public render() {
        return (
            <tr>
                <td className="account">
                    <div className="name">{this.props.account.name}</div>
                    <nav className="desktop">
                        <ul>
                            <li>
                                <a href="">Update Balance</a>
                            </li>
                            <li id="save_{this.props.account.AccountId">
                                <a href="">Save</a>
                            </li>
                            <li id="cancel_{this.props.account.AccountId">
                                <a href="">Cancel</a>
                            </li>
                        </ul>
                    </nav>
                </td>
                <td><span id={"currentBalanceDisplay_" + this.props.account.accountId} className={this.props.account.accountBalance < 0 ? " negative" : ""}>{this.props.account.AccountBalance + (this.props.account.accountBalance < 0 ? "D" : "C") + "R"}</span><input id="currentBalanceEdit_{this.props.account.AccountId" type="number" value={this.props.account.AccountBalance} /></td>
                <td><span id={"availableBalanceDisplay_" + this.props.account.accountId} className={this.props.account.availableBalance < 0 ? " negative" : ""}>{this.props.account.AvailableBalance + (this.props.account.availableBalance < 0 ? "D" : "C") + "R"}</span> <input id="availableBalanceEdit_{this.props.account.AccountId" type="number" value={this.props.account.AvailableBalance} /></td>
            </tr>
        );
    }
}

export default connect()(AccountRow);
