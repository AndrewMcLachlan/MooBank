import { BaseReport } from "./BaseReport";

export interface InOutReport extends BaseReport {
    income: number,
    outgoings: number,
}