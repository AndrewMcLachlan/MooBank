import React from "react";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";

import Overview from "../components/Overview";
import { SecurityProps } from "../global";
import * as securityConfig from "../securityConfiguration";
import { actionCreators } from "../store/Security";
import { State } from "../store/state";

class Home extends React.Component<HomeProps, any> {

    /*    static contextTypes = {
            router: PropTypes.object.isRequired,
        }*/



    constructor(props) {
        super(props);


    }

    public componentDidMount() {

/*        if (securityConfig.loginType === "POPUP") {
            if (this.props.msal.getAccount()) {// avoid duplicate code execution on page load in case of iframe and popup window.
            }
        }
        else if (securityConfig.loginType === "REDIRECT") {
            document.getElementById("SignIn").onclick = () => {
                this.props.msal.loginRedirect(securityConfig.msalRequest);
            };
            if (this.props.msal.getAccount() && !this.props.msal.isCallback(window.location.hash)) {// avoid duplicate code execution on page load in case of iframe and popup window.

            }
        } else {
            console.error("Please set a valid login type");
        }*/
    }

    public render() {
        return this.props.loggedIn ? <Overview /> :
            (
                <section className="login">
                    <header>
                        <h1>Login</h1>
                    </header>
                    <button onClick={this.login}>Login</button>
                </section>
            );
    }

    private login = () => {
        this.signIn();
    }

    private signIn() {

        this.props.msal.loginPopup(securityConfig.msalRequest).then((loginResponse) => {
            //Login Success
            this.props.login(loginResponse.account.name);

        }).catch((error) => {
            console.log(error);
        });
    }

    private authRedirectCallBack(error?, response?) {
        if (error) {
            console.log(error);
        }
        else {
            if (response.tokenType === "access_token") {

            } else {
                console.log("token type is:" + response.tokenType);
            }
        }
    }

    private requiresInteraction(errorCode) {
        if (!errorCode || !errorCode.length) {
            return false;
        }
        return errorCode === "consent_required" ||
            errorCode === "interaction_required" ||
            errorCode === "login_required";
    }
}

export default connect(
    (state: State, ownProps) => ({ ...ownProps, loggedIn: state.security.loggedIn, msal: state.security.msal }),
    (dispatch) => bindActionCreators(actionCreators, dispatch),
)(Home);

interface HomeProps extends SecurityProps {
    loggedIn: boolean;
    login: (name: string) => void;
}
