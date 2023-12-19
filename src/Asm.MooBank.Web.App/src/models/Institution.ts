export interface Institution {
    id: number;
    name: string;
    institutionTypeId?: number;
}

export const emptyInstitution: Institution = {
    id: 0,
    name: "",
};

export interface InstitutionType {
    id: number;
    name: string;
}