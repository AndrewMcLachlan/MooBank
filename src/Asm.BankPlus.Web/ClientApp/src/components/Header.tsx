import { UserAgentApplication } from "msal";
import React from "react";
import { connect } from "react-redux";
import { Link } from "react-router-dom";
import { bindActionCreators } from "redux";

import { actionCreators } from "../store/Security";
import { State } from "../store/state";

class Header extends React.Component<HeaderProps, any> {

    constructor(props) {
        super(props);
    }

    public render() {
        const nav = this.props.loggedIn ?
            (
                <nav>
                    <ul>
                        <li><Link to={this.props.baseUrl}>Home</Link></li>
                        <li className="desktop"><Link to="/accounts">Manage Accounts</Link></li>
                        <li className="desktop"><Link to={this.props.baseUrl + "settings"}>Settings</Link></li>
                        <li><button onClick={this.logout}>Logout</button></li>
                    </ul>
                </nav>
            ) :
            null;
        return (
            <header>
                <Link to={this.props.baseUrl}><img src={this.props.baseUrl + "images/" + this.props.skin + "/moo@2x.png"} alt={this.props.appName} className="desktop" width="76" height="111" /><img src={this.props.baseUrl + "images/" + this.props.skin + "/moo-small@2x.png"} alt={this.props.appName} className="img mobile" width="99" height="32" /></Link>
                {nav}
            </header>
        );
    }

    private logout = () => {
        if (this.props.msal) {
            this.props.msal.logout();
            this.props.logOut();
        }
    }
}

function mapStateToProps(state: State, ownProps) {
    return {
        ...ownProps,
        appName: state.app.appName,
        baseUrl: state.app.baseUrl,
        skin: state.app.skin,
        loggedIn: state.security.loggedIn,
        msal: state.security.msal,
    };
}

export default connect(mapStateToProps,
    (dispatch) => bindActionCreators(actionCreators, dispatch))(Header);

interface HeaderProps {
    appName: string;
    baseUrl: string;
    skin: string;
    loggedIn: boolean;
    msal?: UserAgentApplication;
    logOut: () => void;
}
