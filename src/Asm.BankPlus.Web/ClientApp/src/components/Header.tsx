import * as React from "react";
import { connect } from "react-redux";
import { Link } from "react-router-dom";
import { State } from "../store/state";

const Header = (props) => {

    let nav = props.loggedIn ?
        (
            <nav>
                <ul>
                    <li><Link to={props.baseUrl}>Home</Link></li>
                    <li className="desktop"><Link to="/accounts">Manage Accounts</Link></li>
                    <li className="desktop"><Link to={props.baseUrl + "settings"}>Settings</Link></li>
                    <li><Link to={props.baseUrl + "logout"}>Logout</Link></li>
                </ul>
            </nav>
        ) :
        null;
    return (
        <header>
            <Link to={props.baseUrl}><img src={props.baseUrl + "images/" + props.skin + "/moo@2x.png"} alt={props.appName} className="desktop" width="76" height="111" /><img src={props.baseUrl + "images/" + props.skin + "/moo-small@2x.png"} alt={props.appName} className="img mobile" width="99" height="32" /></Link>
            {nav }
    </header>
    );
}

function mapStateToProps(state: State, ownProps) {
    return {
        ...ownProps,
        appName: state.app.appName,
        baseUrl: state.app.baseUrl,
        skin: state.app.skin,
        loggedIn: state.security.loggedIn,
    };
}

export default connect(mapStateToProps)(Header);
