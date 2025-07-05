import { BaseReport } from "./BaseReport";
import { TrendPoint } from "./TrendPoint";

export interface MonthlyBalancesReport extends BaseReport {
    balances: TrendPoint[];
}
