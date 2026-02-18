import { TagSettings } from "api/types.gen";

export interface BaseReport {
    accountId: string;
    start: string;
    end: string;
}

export type reportInterval = "Monthly" | "Yearly";

export interface TrendReportSettings extends Pick<TagSettings, "applySmoothing"> {
    interval: reportInterval;
}

export const defaultSettings: TrendReportSettings = {
    applySmoothing: false,
    interval: "Monthly",
};
