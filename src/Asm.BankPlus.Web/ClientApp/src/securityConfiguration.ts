import { Configuration } from "msal";

// this can be used for login or token request, however in more complex situations
// this can have diverging options
export const msalRequest = {
    scopes: ["user.read"]
};

export const msalConfig: Configuration = {
    auth: {
        clientId: "045f8afa-70f2-4700-ab75-77ac41b306f7",
        authority: "https://login.microsoftonline.com/30efefb9-9034-4e0c-8c69-17f4578f5924",
    },
    cache: {
        cacheLocation: "localStorage",
        storeAuthStateInCookie: true,
    }
}


// Browser check variables
const ua = window.navigator.userAgent;
const msie = ua.indexOf("MSIE ");
const msie11 = ua.indexOf("Trident/");
const msedge = ua.indexOf("Edge/");
const isIE = msie > 0 || msie11 > 0;
const isEdge = msedge > 0;
//If you support IE, our recommendation is that you sign-in using Redirect APIs
//If you as a developer are testing using Edge InPrivate mode, please add "isEdge" to the if check
// can change this to default an experience outside browser use
export const loginType = isIE ? "REDIRECT" : "POPUP";
