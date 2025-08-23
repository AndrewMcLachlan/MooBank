import { UseQueryResult } from "@tanstack/react-query";
import * as Models from "../models";
import { useApiGet } from "@andrewmclachlan/moo-app";

export const useImporterTypes = (): UseQueryResult<Models.ImporterType[]> => useApiGet<Models.ImporterType[]>(["importertypes"], `api/reference-data/importer-types`);
