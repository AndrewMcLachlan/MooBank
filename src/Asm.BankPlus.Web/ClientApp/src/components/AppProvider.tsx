import React, {useContext } from "react";

export interface AppContextValues {
    appName: string,
    baseUrl: string,
    skin: string,
}

export interface AppProviderProps extends AppContextValues {

}

export const AppContext = React.createContext<AppContextValues | undefined>(undefined);
export const AppProvider: React.FC<React.PropsWithChildren<AppProviderProps>> = ({ children, ...rest }) => {

    return (
        <AppContext.Provider value={{ ...rest }}>
            {children}
        </AppContext.Provider>
    );
};

export const useApp = () => useContext(AppContext);
