import type { UtilityType } from "api/types.gen";

export const getUnit = (accountType: UtilityType): string => {
    switch (accountType) {
        case "Electricity":
            return "kWh";
        case "Water":
            return "kL";
        default:
            return "";
    }
}