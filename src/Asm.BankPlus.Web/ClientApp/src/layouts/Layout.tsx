import "./Layout.scss";

import React, { useState } from "react";
import { Alert } from "../components";
import { Footer } from "./Footer";
import { Header } from "./Header";
import { LayoutProvider } from "../providers";
import { Theme } from "../models/Layout";

export const Layout: React.FC<React.PropsWithChildren<any>> = (props) => {

    const [theme, setTheme] = useState<Theme>(window.localStorage.getItem("theme") as Theme);

    const changeTheme = (theme?: Theme) => {
        setTheme(theme);
        window.localStorage.setItem("theme", theme);
    }

    return (
        <LayoutProvider theme={theme} setTheme={changeTheme}>
            <Alert />
            <main className={theme}>
                <Header />
                <article>
                    {props.children}
                </article>
                <Footer />
            </main>
        </LayoutProvider>
    );
}