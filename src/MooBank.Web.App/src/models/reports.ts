import type { TagSettings } from "api/types.gen";

export type reportInterval = "Monthly" | "Yearly";

export interface TrendReportSettings extends Pick<TagSettings, "applySmoothing"> {
    interval: reportInterval;
}

export const defaultSettings: TrendReportSettings = {
    applySmoothing: false,
    interval: "Monthly",
};
