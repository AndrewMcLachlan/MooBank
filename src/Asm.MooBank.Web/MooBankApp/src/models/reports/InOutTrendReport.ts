import { BaseReport } from "./BaseReport";
import { TrendPoint } from "./TrendPoint";

export interface InOutTrendReport extends BaseReport {
    income: TrendPoint[],
    expenses: TrendPoint[],
}
