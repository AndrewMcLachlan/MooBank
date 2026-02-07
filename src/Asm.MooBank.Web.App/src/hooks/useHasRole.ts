import { useMsal } from "@azure/msal-react";

export const useHasRole = () => {

    const msal = useMsal();

    return (role: string) => (msal.instance.getActiveAccount()?.idTokenClaims?.["roles"]?.includes(role)) ?? false;
};