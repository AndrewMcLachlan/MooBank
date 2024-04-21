import { AccountBase, TopLevelAccount } from "./Account";

export interface AssetBase extends AccountBase, TopLevelAccount {
    purchasePrice?: number;
}

export interface NewAsset extends AssetBase {
}

export interface Asset extends AssetBase {
}

export const emptyAsset: NewAsset = {
    id: "",
    name: "",
    purchasePrice: 0,
    accountGroupId: "",
    currentBalance: 0,
    currentBalanceLocalCurrency: 0,
    currency: "",
    shareWithFamily: false,
}
