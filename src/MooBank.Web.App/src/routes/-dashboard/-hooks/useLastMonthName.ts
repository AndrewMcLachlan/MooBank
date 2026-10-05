import { useIsAtLeast } from "@andrewmclachlan/moo-ds";
import { lastMonthName } from "utils/dateFns";

export const useLastMonthName = () => lastMonthName(!useIsAtLeast("md"));
