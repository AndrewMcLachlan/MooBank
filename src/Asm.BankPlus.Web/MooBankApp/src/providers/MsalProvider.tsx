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

export const msalConfig: msal.Configuration = {
    auth: {
        clientId: "045f8afa-70f2-4700-ab75-77ac41b306f7",
        authority: "https://login.microsoftonline.com/30efefb9-9034-4e0c-8c69-17f4578f5924",
        redirectUri: window.location.origin,
        navigateToLoginRequestUrl: true,
        postLogoutRedirectUri: window.location.origin,
    },
    cache: {
        cacheLocation: "sessionStorage", // This configures where your cache will be stored
        storeAuthStateInCookie: false, // Set this to "true" if you are having issues on IE11 or Edge
    },
    system: {
        loggerOptions: {
            loggerCallback: (level, message, containsPii) => {
                if (containsPii) {
                    return;
                }
                switch (level) {
                    case msal.LogLevel.Error:
                        console.error(message);
                        return;
                    case msal.LogLevel.Info:
                        console.info(message);
                        return;
                    case msal.LogLevel.Verbose:
                        console.debug(message);
                        return;
                    case msal.LogLevel.Warning:
                        console.warn(message);
                        return;
                    default:
                        return;
                }
            }
        }
    }
};

// Add here scopes for id token to be used at MS Identity Platform endpoints.
export const loginRequest: msal.RedirectRequest = {
    scopes: ["openid", "profile", "User.Read"],
    //forceRefresh: false // Set this to "true" to skip a cached token and go to the server to get a new token
};

// Add here scopes for id token to be used at MS Identity Platform endpoints.
export const apiRequest: msal.SilentRequest = {
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
}: MsalProviderProps) => {
    const [isAuthenticated, setIsAuthenticated] = useState<boolean>(false);
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
            console.info("No accounts detected");
            return null;
        }

        if (currentAccounts.length > 1) {
            // Add choose account code here
            console.warn("Multiple accounts detected, need to add choose account code.");
            return currentAccounts[0];
        } else if (currentAccounts.length === 1) {
            return currentAccounts[0];
        }
    }


    if (!publicClient) {
        const pc: msal.PublicClientApplication = new msal.PublicClientApplication(config);
        
        pc.addEventCallback((message) => { if (message?.eventType === msal.EventType.HANDLE_REDIRECT_START) setLoading(true); });

        pc.handleRedirectPromise().then((response) => {
            if (response) {
                setUser(getAccount(pc));
                setIsAuthenticated(true);
                if (response.accessToken) {
                    setToken(response.accessToken);
                }
            }
        }).catch(error => {
            console.error(error);
            setLoginError(error);
        }).finally((() => setLoading(false)));

        if (getAccount(pc)) {
            setUser(getAccount(pc));
            setIsAuthenticated(true);
        }

        setPublicClient(pc);
    }

    const login = async (method: LoginMethod) => {
        if (loading) return;
        setLoading(true);

        const signInType = (isIE || isEdge) ? "loginRedirect" : method;
        if (signInType === "loginPopup") {
            setPopupOpen(true);

            try {
                await publicClient.loginPopup(loginRequest);

                if (getAccount(publicClient)) {
                    setUser(getAccount(publicClient));
                    setIsAuthenticated(true);
                }
            } catch (error: any) {
                console.error(error);
                setLoginError(error);
            } finally {
                setPopupOpen(false);
            }
        } else if (signInType === "loginRedirect") {

            publicClient.loginRedirect(loginRequest);
        }
    }

    const logout = () => {
        publicClient.loginRedirect();
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
            catch (error: any) {
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
        catch (error) {

            try {
                setLoading(true);
                publicClient.acquireTokenRedirect(request);
            }
            catch (error: any) {
                console.log(error);
                setLoginError(error);
            }
        }
    }

    const getToken = async (request: any, method: LoginMethod): Promise<string> => {
        const signInType = (isIE || isEdge) ? "loginRedirect" : method;
        if (signInType === "loginRedirect") {
            return await getTokenRedirect(request);
        } else {
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
