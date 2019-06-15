import React from "react";
import { connect } from "react-redux";
import { Route } from "react-router-dom";
import { bindActionCreators } from "redux";

import Layout from "./components/Layout";
import { SecurityProps } from "./global";
import Home from "./pages/Home";
import ManageAccounts from "./pages/ManageAccounts";
import Settings from "./pages/Settings";
import * as securityConfig from "./securityConfiguration";
import { actionCreators } from "./store/Security";
import { State } from "./store/state";

class App extends React.Component<Props, any> {

    constructor(props) {
        super(props);

        // this.props.handleRedirectCallback(this.authRedirectCallBack);
    }

    public componentDidMount() {
        this.props.msal.acquireTokenSilent(securityConfig.msalRequest).then((tokenResponse) => {
            this.props.login(tokenResponse.account.name);
        }).catch();
        // TODO: Possibly force login at this stage
    }

    public render() {
        return (
            <Layout>
                <Route exact={true} path="/" component={Home} />
                <Route path="/accounts" component={ManageAccounts} />
                <Route path="/settings" component={Settings} />
            </Layout>
        );
    }
}

export default connect(
    (state: State, ownProps): SecurityProps => ({ ...ownProps, msal: state.security.msal }),
    (dispatch) => bindActionCreators(actionCreators, dispatch),
)(App);

interface Props extends SecurityProps {
    login: (name: string) => void;
}
