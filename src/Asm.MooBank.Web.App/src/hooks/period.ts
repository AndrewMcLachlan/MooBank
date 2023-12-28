import { useLocalStorage } from "@andrewmclachlan/mooapp";
import { parseISO } from "date-fns/parseISO";
import { Period, lastMonth } from "helpers/dateFns";

export const usePeriod = (): [period: Period, setPeriod: (value: Period) => void] => {
    const [period, setPeriod] = useLocalStorage("period", lastMonth);

    period.startDate = period.startDate && typeof period.startDate === "string" ? parseISO(period.startDate as any) : period.startDate;
    period.endDate = period.endDate && typeof period.endDate === "string" ? parseISO(period.endDate as any) : period.endDate;

    return [period, setPeriod];
}
