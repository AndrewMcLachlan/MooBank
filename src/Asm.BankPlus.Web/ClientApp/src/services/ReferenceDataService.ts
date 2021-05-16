import * as Models from "../models";
import { useApiGet } from "./useApiGet";

export const useImporterTypes = () => useApiGet<Models.ImporterType[]>(["importertypes"], `api/referencedata/importertypes`);
