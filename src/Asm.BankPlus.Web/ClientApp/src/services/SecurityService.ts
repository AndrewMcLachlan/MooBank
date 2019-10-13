import { Configuration, UserAgentApplication, AuthenticationParameters } from "msal";

// this can be used for login or token request, however in more complex situations
// this can have diverging options
export const msalRequest : IMSalRequest = {
    scopes:["api://045f8afa-70f2-4700-ab75-77ac41b306f7/user_impersonation"],
};

export interface IMSalRequest {
    scopes: string[];
}

export const useSecurityService = (): SecurityService => new SecurityService();

export class SecurityService {

    private static msal: UserAgentApplication;

    public get userName(): string {
        const account = SecurityService.msal.getAccount();
        return (account ? account.name : "");
    }

    constructor() {
        if (!SecurityService.msal) {
            SecurityService.msal = new UserAgentApplication(msalConfig);
            SecurityService.msal.handleRedirectCallback((error) => {
                if (error) {
                    throw error;
                }
            });
        }
    }

    public isUserLoggedIn(): boolean {
        const idToken = sessionStorage.getItem('msal.idtoken');
        const hasToken = idToken !== null && idToken !== '';

        const error = sessionStorage.getItem('msal.error');
        const hasError = error === 'login_required';

        return hasToken && !hasError;
    }

    public login() {

        if (this.isUserLoggedIn()) {
            return;
        }

        try {
            SecurityService.msal.loginRedirect(msalRequest);
        }
        catch(e) {
            console.log(e);
        }
    }

    public logout() {
        SecurityService.msal.logout();
    }

    public async getToken(): Promise<string> {

            const tokenRequest = {... msalRequest, forceRefresh: false};

            try {
                if (!SecurityService.msal) {
                    return Promise.reject();
                }
                let response = await SecurityService.msal.acquireTokenSilent(tokenRequest);
                return response.accessToken;
            }

            catch (err) {
                switch (err.name) {

                    case "InteractionRequiredAuthError":
                        try {
                            if (!SecurityService.msal) {
                                return Promise.reject();
                            }
                            const response = await SecurityService.msal.acquireTokenPopup(tokenRequest);
                            return response.accessToken;
                        }
                        catch  {
                            this.login();
                        }
                        break;
                    case 'invalid_grant':
                    case "login_required":
                        this.login();
                        break;
                }
                return Promise.resolve("");

            }
    }
}

export const msalConfig: Configuration = {
    auth: {
        clientId: "045f8afa-70f2-4700-ab75-77ac41b306f7",
        authority: "https://login.microsoftonline.com/30efefb9-9034-4e0c-8c69-17f4578f5924",
        redirectUri: getBaseUrl(),
        navigateToLoginRequestUrl: false
    },
    cache: {
        cacheLocation: "sessionStorage",
        storeAuthStateInCookie: true
    },
};

function getBaseUrl():string {
    let location = window.location;
    let baseURL = location.protocol + "//" + location.hostname;

    if (typeof location.port !== "undefined" && location.port !== "")
    {
        baseURL += ":" + location.port;
    }
    return baseURL;
}


// Browser check variables
const ua = window.navigator.userAgent;
const msie = ua.indexOf("MSIE ");
const msie11 = ua.indexOf("Trident/");
const msedge = ua.indexOf("Edge/");
const isIE = msie > 0 || msie11 > 0;
const isEdge = msedge > 0;
// If you support IE, our recommendation is that you sign-in using Redirect APIs
// If you as a developer are testing using Edge InPrivate mode, please add "isEdge" to the if check
// can change this to default an experience outside browser use
export const loginType = isIE ? "REDIRECT" : "POPUP";
