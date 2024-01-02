import { useLocalStorage } from "@andrewmclachlan/mooapp";

export const useBudgetYear = () => useLocalStorage("budget-year", new Date().getFullYear());