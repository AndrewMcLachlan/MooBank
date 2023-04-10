import { BaseReport } from "./BaseReport";
import { TrendPoint } from "./TrendPoint";

export interface TagTrendReport extends BaseReport {
    tagId: number,
    tagName: string,
    months: TrendPoint[],
}
