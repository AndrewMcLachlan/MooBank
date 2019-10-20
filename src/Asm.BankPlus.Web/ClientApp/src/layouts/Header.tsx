import React from "react";
import { Link } from "react-router-dom";

import { State } from "../store/state";
import { useSelector } from "react-redux";

export const Header : React.FC<HeaderProps> = (props) => {

const appState = useAppState();

        return (
            <header>
                <div className="container">
                <Link to="/"><img src={appState.baseUrl + "images/" + appState.skin + "/moo@2x.png"} alt={appState.appName} className="desktop" width="76" height="111" /><img src={appState.baseUrl + "images/" + appState.skin + "/moo-small@2x.png"} alt={appState.appName} className="img mobile" width="99" height="32" /></Link>
                </div>
                <nav>
                    <ul>
                        <li><Link to="/">Home</Link></li>
                        <li className="desktop"><Link to="/accounts">Manage Accounts</Link></li>
                        <li className="desktop"><Link to="/settings">Settings</Link></li>
                        <li><button>Logout</button></li>
                    </ul>
                </nav>
            </header>
        );


}

function useAppState() {
    return {
        appName:     useSelector((state: State) => state.app.appName),
        baseUrl:     useSelector((state: State) => state.app.baseUrl),
        skin: useSelector((state: State) => state.app.skin),
    };
}

interface HeaderProps {
}
