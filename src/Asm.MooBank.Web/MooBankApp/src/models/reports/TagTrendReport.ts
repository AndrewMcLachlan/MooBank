import { TagSettings } from "../Tag";
import { BaseReport } from "./BaseReport";
import { TrendPoint } from "./TrendPoint";

export interface TagTrendReport extends BaseReport {
    tagId: number,
    tagName: string,
    months: TrendPoint[],
    average: number,
    offsetAverage: number,
}

export interface TagTrendReportSettings extends Pick<TagSettings, "applySmoothing"> {

}

export const defaultSettings: TagTrendReportSettings = {
    applySmoothing: false,
}