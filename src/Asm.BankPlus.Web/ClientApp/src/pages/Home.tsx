import Msal, { UserAgentApplication } from "msal";
import React from "react";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";

import { actionCreators } from "../store/Security";
import { State } from "../store/state";
import Overview from "../components/Overview";

class Home extends React.Component<HomeProps, any> {

    /*    static contextTypes = {
            router: PropTypes.object.isRequired,
        }*/

    constructor(props) {
        super(props);

        this.myMSALObj.handleRedirectCallback(this.authRedirectCallBack);
    }

    public componentDidMount() {

        if (this.loginType === "POPUP") {
            if (this.myMSALObj.getAccount()) {// avoid duplicate code execution on page load in case of iframe and popup window.
            }
        }
        else if (this.loginType === "REDIRECT") {
            document.getElementById("SignIn").onclick = () => {
                this.myMSALObj.loginRedirect(this.requestObj);
            };
            if (this.myMSALObj.getAccount() && !this.myMSALObj.isCallback(window.location.hash)) {// avoid duplicate code execution on page load in case of iframe and popup window.

            }
        } else {
            console.error("Please set a valid login type");
        }
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

    private msalConfig: Msal.Configuration = {
        auth: {
            clientId: "045f8afa-70f2-4700-ab75-77ac41b306f7",
            authority: "https://login.microsoftonline.com/30efefb9-9034-4e0c-8c69-17f4578f5924",
        },
        cache: {
            cacheLocation: "localStorage",
            storeAuthStateInCookie: true,
        }
    };

    // this can be used for login or token request, however in more complex situations
    // this can have diverging options
    private requestObj = {
        scopes: ["user.read"]
    };

    private signIn() {

        this.myMSALObj.loginPopup(this.requestObj).then((loginResponse) => {
            //Login Success
            this.props.logIn(this.myMSALObj);

        }).catch((error) => {
            console.log(error);
        });
    };

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
    };

    private requiresInteraction(errorCode) {
        if (!errorCode || !errorCode.length) {
            return false;
        }
        return errorCode === "consent_required" ||
            errorCode === "interaction_required" ||
            errorCode === "login_required";
    };

    private myMSALObj = this.props.msal || new UserAgentApplication(this.msalConfig);

    // Browser check variables
    private ua = window.navigator.userAgent;
    private msie = this.ua.indexOf("MSIE ");
    private msie11 = this.ua.indexOf("Trident/");
    private msedge = this.ua.indexOf("Edge/");
    private isIE = this.msie > 0 || this.msie11 > 0;
    private isEdge = this.msedge > 0;
    //If you support IE, our recommendation is that you sign-in using Redirect APIs
    //If you as a developer are testing using Edge InPrivate mode, please add "isEdge" to the if check
    // can change this to default an experience outside browser use
    private loginType = this.isIE ? "REDIRECT" : "POPUP";
}

export default connect(
    (state: State, ownProps) => ({ ...ownProps, loggedIn: state.security.loggedIn, msal: state.security.msal }),
    (dispatch) => bindActionCreators(actionCreators, dispatch)
)(Home);

interface HomeProps {
    loggedIn: boolean;
    msal?: UserAgentApplication;
    logIn: (data: UserAgentApplication) => void;
}
