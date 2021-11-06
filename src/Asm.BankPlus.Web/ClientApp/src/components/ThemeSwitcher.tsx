import "./ThemeSwitcher.scss";

import { Form } from "react-bootstrap";
import { useLayout } from "../providers/LayoutProvider"

export const ThemeSwitcher = () => {

    const { theme, setTheme, defaultTheme } = useLayout();

    return (
        <Form.Check className="theme-switcher" type="switch" checked={theme === "light" || defaultTheme === "light"} onChange={e => setTheme(e.currentTarget.checked ? "light" : "dark")} />
    );
}