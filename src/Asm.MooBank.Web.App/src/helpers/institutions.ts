import type { Institution, InstitutionType } from "api/types.gen";

export const InstitutionTypes: InstitutionType[] = ["Bank", "SuperannuationFund", "Broker", "CreditUnion", "BuildingSociety", "InvestmentFund", "Government", "Other"];

export const institutionTypeOptions = InstitutionTypes.map(t => ({ value: t, label: t.replace(/([A-Z])/g, " $1").trim() }));

export const emptyInstitution: Institution = {
    id: 0,
    name: "",
    institutionType: "Bank",
};
