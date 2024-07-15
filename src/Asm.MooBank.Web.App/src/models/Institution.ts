export const InstitutionTypes = ["Bank", "SuperannuationFund", "Broker", "CreditUnion", "BuildingSociety", "InvestmentFund", "Government", "Other"] as InstitutionType[];
export type InstitutionType = "Bank" | "SuperannuationFund" | "Broker" | "CreditUnion" | "BuildingSociety" | "InvestmentFund" | "Government" | "Other";

export const institutionTypeOptions = InstitutionTypes.map(t => ({ value: t, label: t.replace(/([A-Z])/g, " $1").trim() }));

export interface Institution {
    id: number;
    name: string;
    institutionType?: InstitutionType;
}

export const emptyInstitution: Institution = {
    id: 0,
    name: "",
};
