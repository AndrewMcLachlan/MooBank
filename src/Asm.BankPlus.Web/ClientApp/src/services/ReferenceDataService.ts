import * as Models from "../models";
import { useApiQuery } from "./useApiQuery";

export const useImporterTypes = () => useApiQuery<Models.ImporterType[]>(["tags"], `api/referencedata/importertypes`);
