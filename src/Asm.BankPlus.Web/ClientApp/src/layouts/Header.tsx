import React from "react";
import { Link } from "react-router-dom";

import { useApp, useMsal } from "../components";

export const Header: React.FC<HeaderProps> = (props) => {

    const appState = useApp();

    const { loading, isAuthenticated, login, logout } = useMsal();

    /*if (!isAuthenticated && !loading) {
        login("loginRedirect");
    }*/

    return (
        <header>
            <div className="container">
                <Link to="/"><img src={appState.baseUrl + "images/" + appState.skin + "/moo@2x.png"} alt={appState.appName} className="desktop" width="76" height="111" /><img src={appState.baseUrl + "images/" + appState.skin + "/moo-small@2x.png"} alt={appState.appName} className="img mobile" width="99" height="32" /></Link>
            </div>
            <nav>
                <ul>
                    {!isAuthenticated && <li><a onClick={() => login("loginRedirect")}>Login</a></li>}
                    <li><Link to="/">Home</Link></li>
                    <li className="desktop"><Link to="/accounts">Manage Accounts</Link></li>
                    <li className="desktop"><Link to="/settings">Settings</Link></li>
                    {isAuthenticated && <li><button onClick={() => logout()}>Logout</button></li>}
                </ul>
            </nav>
        </header>
    );
}

interface HeaderProps {
}
