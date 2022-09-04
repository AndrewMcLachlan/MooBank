import React, { createContext } from "react";
import { useContext } from "react";
import * as Models from "../models";

export const LayoutContext = createContext<Models.Layout>({ defaultTheme: window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches ? "dark" : "light" });

export const LayoutProvider: React.FC<React.PropsWithChildren<LayoutProps>> = ({ theme, setTheme, children }) => {


    return (
        <LayoutContext.Provider value={{ theme, setTheme, defaultTheme: window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches ? "dark" : "light" }}>
            {children}
        </LayoutContext.Provider>
    );
}

export const useLayout = () => useContext(LayoutContext);

export interface LayoutProps extends Omit<Models.Layout, "defaultTheme"> {
}

LayoutProvider.displayName = "LayoutProvider";