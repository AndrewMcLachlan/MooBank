import { useLocalStorage } from "@andrewmclachlan/mooapp";
import { parseISO } from "date-fns/parseISO";
import { Period, lastMonth } from "helpers/dateFns";
import { periodOptions } from "models";

export const useCustomPeriod = (): [period: Period, setPeriod: (value: Period) => void] => {
    const [period, setPeriod] = useLocalStorage("period", lastMonth);

    period.startDate = period.startDate && typeof period.startDate === "string" ? parseISO(period.startDate as any) : period.startDate;
    period.endDate = period.endDate && typeof period.endDate === "string" ? parseISO(period.endDate as any) : period.endDate;

    return [period, setPeriod];
}

export const getPeriod = (): Period => {
    
    const periodId = JSON.parse(localStorage.getItem("period-id")) ?? "1";

    if (periodId !== "-1") {
        return periodOptions.find(o => o.value === periodId) ?? periodOptions[0];
    }

    const period = localStorage.getItem("period") ? JSON.parse(localStorage.getItem("period") as string) : null;
    period.startDate = period.startDate && typeof period.startDate === "string" ? parseISO(period.startDate as any) : period.startDate;
    period.endDate = period.endDate && typeof period.endDate === "string" ? parseISO(period.endDate as any) : period.endDate;

    return period;
}
