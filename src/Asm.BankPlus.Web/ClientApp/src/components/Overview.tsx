import React from "react";
import { connect } from "react-redux";
import { bindActionCreators, Dispatch } from "redux";

import { actionCreators } from "../store/Accounts";
import * as Model from "../store/state";
import { State } from "../store/state";
import AccountRow from "./AccountRow";
import VirtualAccountRow from "./VirtualAccountRow";

class Overview extends React.Component<OverviewProps, any> {

    constructor(props) {
        super(props);
    }

    public componentDidMount() {
        this.props.requestAccounts();
    }

    public componentDidUpdate() {
        // this.props.requestAccounts();
    }

    public render() {

        const virtualAccounts = [];
        for (const account of this.props.virtualAccounts) {
            virtualAccounts.push(<VirtualAccountRow key={account.virtualAccountId} account={account} />);
        }

        const accounts = [];
        for (const account of this.props.accounts) {
            accounts.push(<AccountRow key={account.accountId} account={account} />);
        }

        return (
            <section>
                <h2>Virtual Accounts</h2>

                <table id="virtualAccounts" className="accounts">
                    <thead>
                        <tr>
                            <th>Name</th>
                            <th>Balance</th>
                        </tr>
                    </thead>
                    <tbody>
                        {virtualAccounts}
                    </tbody>
                </table>

                <h2>Accounts</h2>

                <table className="accounts">
                    <thead>
                        <tr>
                            <th>Name</th>
                            <th>Current Balance</th>
                            <th>Available Balance</th>
                        </tr>
                    </thead>
                    <tbody>
                        {accounts}
                    </tbody>
                </table>
            </section>
        );
    }
}

export default connect(
    (state: Model.State, ownProps) => ({ ...ownProps, accounts: state.accounts.accounts, virtualAccounts: state.accounts.virtualAccounts }),
    (dispatch) => bindActionCreators(actionCreators, dispatch),
)(Overview);

interface OverviewProps {
    requestAccounts: () => Promise<(dispatch: Dispatch, getState: () => State) => void>;
    accounts: Model.RealAccount[];
    virtualAccounts: Model.VirtualAccount[];
}
