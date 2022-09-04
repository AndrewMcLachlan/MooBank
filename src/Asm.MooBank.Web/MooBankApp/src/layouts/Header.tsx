import React from "react";
import { Container } from "react-bootstrap";
import { Link } from "react-router-dom";
import { ThemeSwitcher } from "../components";
import { useMsal } from "../providers";

export const Header: React.FC<HeaderProps> = (props) => {

    const { isAuthenticated, login, logout } = useMsal();

    return (
        <header>
            <Container>
                <Link to="/">
                    <img src="/logo.svg" alt="MooPlan" height="80" className="logo" />
                </Link>
                <h1>MooBank</h1>
            </Container>
            <nav>
                <ul>
                    <li><ThemeSwitcher /></li>
                    {!isAuthenticated && <li><button onClick={() => login("loginRedirect")}>Login</button></li>}
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
