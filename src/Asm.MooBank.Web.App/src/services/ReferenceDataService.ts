import { UseQueryResult } from "@tanstack/react-query";
import * as Models from "../models";
import { useApiGet } from "@andrewmclachlan/mooapp";

export const useImporterTypes = (): UseQueryResult<Models.ImporterType[]> => useApiGet<Models.ImporterType[]>(["importertypes"], `api/reference-data/importer-types`);
export const useInstitutionTypes = (): UseQueryResult<Models.InstitutionType[]> => useApiGet<Models.InstitutionType[]>(["institutiontypes"], `api/reference-data/institution-types`);