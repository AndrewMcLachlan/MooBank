import type { Asset } from "api/types.gen";

export const emptyAsset: Partial<Asset> = {
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
};
