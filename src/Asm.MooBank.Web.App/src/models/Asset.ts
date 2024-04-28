import { Instrument, TopLevelAccount } from "./Account";

export interface AssetBase extends Instrument, TopLevelAccount {
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
    groupId: "",
    controller: "Manual",
    currentBalance: 0,
    currentBalanceLocalCurrency: 0,
    currency: "",
    shareWithFamily: false,
    virtualInstruments: [],
}
