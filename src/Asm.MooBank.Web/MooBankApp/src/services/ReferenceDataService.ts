import * as Models from "../models";
import { useApiGet } from "@andrewmclachlan/mooapp";

export const useImporterTypes = () => useApiGet<Models.ImporterType[]>(["importertypes"], `api/referencedata/importertypes`);
