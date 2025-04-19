import { TagSettings } from "../Tag";
import { BaseReport, reportInterval } from "./BaseReport";
import { TrendPoint } from "./TrendPoint";

export interface TagTrendReport extends BaseReport {
    tagId: number,
    tagName: string,
    months: TrendPoint[],
    average: number,
    offsetAverage: number,
}

export interface TrendReportSettings extends Pick<TagSettings, "applySmoothing"> {
    interval: reportInterval,
}

export const defaultSettings: TrendReportSettings = {
    applySmoothing: false,
    interval: "Monthly",
}