import React, { useContext } from "react";

import { Asset } from "models";

export interface AssetProviderProps {
    asset: Asset;
}

export const AssetContext = React.createContext<Asset>(undefined);
export const AssetProvider: React.FC<React.PropsWithChildren<AssetProviderProps>> = ({ asset: account, children }) => {

    return (
        <AssetContext.Provider value={account}>
            {children}
        </AssetContext.Provider>
    );
};

export const useAsset = () => useContext(AssetContext);
