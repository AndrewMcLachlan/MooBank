import React, { useState, useContext } from "react";
import * as msal from "@azure/msal-browser";

declare global {
    interface Window {
        getToken: (request: any, method: LoginMethod) => Promise<string>,
        token: string,
    }
}

const ua = window.navigator.userAgent;
const msie = ua.indexOf("MSIE ");
const msie11 = ua.indexOf("Trident/");
const msedge = ua.indexOf("Edge/");
const isIE = msie > 0 || msie11 > 0;
const isEdge = msedge > 0;

export type LoginMethod = "loginRedirect" | "loginPopup";

export interface MsalContextValues {
    isAuthenticated: boolean,
    user: msal.AccountInfo,
    token: string,
    loading: boolean,
    popupOpen: boolean,
    loginError: any,
    login: (method: "loginRedirect" | "loginPopup") => Promise<void>,
    logout: () => void,
    getToken: (request: any, method: LoginMethod) => Promise<string>,
}
  
export const msalConfig = {
    auth: {
        clientId: "045f8afa-70f2-4700-ab75-77ac41b306f7",
        authority: "https://login.microsoftonline.com/30efefb9-9034-4e0c-8c69-17f4578f5924",
    },
    cache: {
        cacheLocation: "localStorage", // This configures where your cache will be stored
        storeAuthStateInCookie: false, // Set this to "true" if you are having issues on IE11 or Edge
    }
};

// Add here scopes for id token to be used at MS Identity Platform endpoints.
export const loginRequest = {
    scopes: ["openid", "profile", "User.Read"],
    forceRefresh: false // Set this to "true" to skip a cached token and go to the server to get a new token
};

// Add here scopes for id token to be used at MS Identity Platform endpoints.
export const apiRequest = {
    scopes: ["api://bankplus.mclachlan.family/api.read"],
    forceRefresh: false // Set this to "true" to skip a cached token and go to the server to get a new token
};

export interface MsalProviderProps {
    children: any,
    config: msal.Configuration,
}

export const MsalContext = React.createContext<MsalContextValues | undefined>(undefined);
export const MsalProvider = ({
    children,
    config,
}:MsalProviderProps) => {
    const [isAuthenticated, setIsAuthenticated] = useState<boolean>();
    const [user, setUser] = useState<msal.AccountInfo>();
    const [token, setToken] = useState<string>();
    const [publicClient, setPublicClient] = useState<msal.PublicClientApplication>();
    const [loading, setLoading] = useState(false);
    const [popupOpen, setPopupOpen] = useState(false);
    const [loginError, setLoginError] = useState(false);

    const getAccount = (pc: msal.PublicClientApplication): msal.AccountInfo => {
        // need to call getAccount here?
        const currentAccounts = pc.getAllAccounts();
        if (currentAccounts === null) {
            console.log("No accounts detected");
            return null;
        }

        if (currentAccounts.length > 1) {
            // Add choose account code here
            console.log("Multiple accounts detected, need to add choose account code.");
            return currentAccounts[0];
        } else if (currentAccounts.length === 1) {
            return currentAccounts[0];
        }
    }

    
    if (!publicClient) {
        const pc: msal.PublicClientApplication = new msal.PublicClientApplication(config);

        pc.handleRedirectPromise().then((response) => 
        {
            setLoading(false);
            if (response) {
                setUser(getAccount(pc));
                setIsAuthenticated(true);
                if(response.accessToken) {
                  setToken(response.accessToken);
                }
            } 
        }).catch(error => {
            console.log(error);
            setLoginError(error);
        });

        if (getAccount(pc)) {
            setUser(getAccount(pc));
            setIsAuthenticated(true);
        }
        
        setPublicClient(pc);
    }

    const login = async (method: LoginMethod) => {
        const signInType = (isIE || isEdge) ? "loginRedirect" : method;
        if (signInType === "loginPopup") {
            setPopupOpen(true);

            try {
                await publicClient.loginPopup(loginRequest);

                if (getAccount(publicClient)) {
                    setUser(getAccount(publicClient));
                    setIsAuthenticated(true);
                }
            } catch (error) {
                console.log(error);
                setLoginError(error);
            } finally {
                setPopupOpen(false);
            }
        } else if (signInType === "loginRedirect") {
            setLoading(true);

            publicClient.loginRedirect(loginRequest)
        }
    }

    const logout = () => {
        publicClient.logout();
    }

    const getTokenPopup = async (request: any) => {
        try {
            request.account = user;
            if (!request.account || request.account === null) return "No Account";

            var token = (await publicClient.acquireTokenSilent(request)).accessToken;
            setToken(token);
            return token;
        } catch (error) {
            try {
                setPopupOpen(true);
                
                const response = await publicClient.acquireTokenPopup(request);
                setToken(response.accessToken);
                return response.accessToken;
            }
            catch (error) {
                console.log(error);
                setLoginError(error);
            }
            finally {
                setPopupOpen(false);
            }
        }
    }

    const getTokenRedirect = async (request: msal.SilentRequest): Promise<string> => {
        try {
            request.account = user;
            if (!request.account || request.account === null) return "No Account";

            var token = (await publicClient.acquireTokenSilent(request)).accessToken;
            setToken(token);
            return token;
        }
        catch(error) {
               
            try{
                setLoading(true);
                publicClient.acquireTokenRedirect(request);
            }
            catch(error) { 
                console.log(error);
                setLoginError(error);
            }
        }
    }

    const getToken = async (request: any, method: LoginMethod): Promise<string> => {
        const signInType = (isIE || isEdge)? "loginRedirect" : method;
        if(signInType === "loginRedirect") {
            return await getTokenRedirect(request);
        } else
        {
            return await getTokenPopup(request);
        }
    }

    window.getToken = getToken;

    return (
        <MsalContext.Provider
            value={{
                isAuthenticated,
                user,
                token,
                loading,
                popupOpen,
                loginError,
                login,
                logout,
                getToken
            }}
        >
            {children}
        </MsalContext.Provider>
    );
};

export const useMsal = () => useContext(MsalContext);
